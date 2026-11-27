using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Saving and loading characters online (see SaveHandler and GameSession).
/// Each player's machine owns its character, so:
/// - a joining player tells the host who they are, and the host sends back their saved character, if the session has one;
/// - players send their character to the host every so often, when the host saves, and before they leave,
///   so the host always has a recent copy to write when it saves or when someone leaves.
/// Characters travel as JSON in chunks, to stay under the transport's message size.
/// </summary>
public partial class NetworkPlayer
{
    private const int CharacterChunkSize = 4000;
    private const float CharacterPushInterval = 30f;
    private const float SaveCollectTimeout = 3f;

    private float characterPushTimer = CharacterPushInterval;
    private uint nextTransferId;
    private readonly Dictionary<uint, byte[][]> incomingTransfers = new();

    /// <summary>Host side: when this player's character last arrived.</summary>
    public float LastCharacterReceivedAt { get; private set; } = float.NegativeInfinity;

    public static NetworkPlayer Local => PlayerStats.Instance != null ? PlayerStats.Instance.GetComponent<NetworkPlayer>() : null;

    private void StartSaveSession()
    {
        if (!IsOwner) return;
        if (IsServer) StartCoroutine(SaveHandler.ApplyCharacterWhenReady(GameSession.TakeHostCharacter()));
        else SubmitPlayerIdServerRpc(SaveHandler.LocalPlayerId);
    }

    // Owner side, every frame.
    private void UpdateSaves()
    {
        if (IsServer) return;
        characterPushTimer -= Time.unscaledDeltaTime;
        if (characterPushTimer > 0f) return;
        characterPushTimer = CharacterPushInterval;
        PushCharacter();
    }

    #region Loading a joining player's character

    [ServerRpc]
    private void SubmitPlayerIdServerRpc(string playerId)
    {
        PlayerSaveRecord record = GameSession.RegisterPlayer(OwnerClientId, playerId);
        if (record == null) return;
        SendChunks(JsonUtility.ToJson(record.data), (id, index, count, chunk) => LoadCharacterChunkClientRpc(id, index, count, chunk, ToOwner()));
    }

    [ClientRpc]
    private void LoadCharacterChunkClientRpc(uint transferId, int index, int count, byte[] chunk, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner || IsServer) return;
        if (TryReceiveChunk(transferId, index, count, chunk, out string json))
            StartCoroutine(SaveHandler.ApplyCharacterWhenReady(JsonUtility.FromJson<SaveData>(json)));
    }

    #endregion

    #region Sending characters to the host

    /// <summary>Owner side: sends this player's character, as it is now, to the host.</summary>
    public void PushCharacter()
    {
        if (!IsSpawned || !IsOwner || IsServer) return;
        SaveData character = SaveHandler.BuildCharacter();
        if (character == null) return;
        characterPushTimer = CharacterPushInterval;
        SendChunks(JsonUtility.ToJson(character), (id, index, count, chunk) => CharacterChunkServerRpc(id, index, count, chunk));
    }

    [ServerRpc]
    private void CharacterChunkServerRpc(uint transferId, int index, int count, byte[] chunk)
    {
        if (!TryReceiveChunk(transferId, index, count, chunk, out string json)) return;
        GameSession.StoreCharacter(OwnerClientId, GetComponent<PlayerStats>().Name, JsonUtility.FromJson<SaveData>(json));
        LastCharacterReceivedAt = Time.unscaledTime;
    }

    [ClientRpc]
    private void RequestCharacterClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsOwner && !IsServer) PushCharacter();
    }

    #endregion

    #region Host side: saving the session

    /// <summary>
    /// Saves the session to a slot. Online, the host first asks every player for their character and waits
    /// a few seconds for them; anyone who doesn't answer in time is saved from their last copy.
    /// </summary>
    public static void SaveSession(int slot, Action<bool> onDone)
    {
        NetworkPlayer host = Local;
        if (!NetworkSession.IsOnline || host == null || !host.IsServer)
        {
            onDone?.Invoke(!NetworkSession.IsClientOnly && SaveHandler.WriteSession(slot));
            return;
        }
        host.StartCoroutine(host.CollectAndSave(slot, onDone));
    }

    private IEnumerator CollectAndSave(int slot, Action<bool> onDone)
    {
        float requestedAt = Time.unscaledTime;
        List<NetworkPlayer> guests = GuestPlayers();
        foreach (NetworkPlayer guest in guests) guest.RequestCharacterClientRpc(guest.ToOwner());

        while (Time.unscaledTime - requestedAt < SaveCollectTimeout && guests.Exists(g => g != null && g.LastCharacterReceivedAt < requestedAt))
            yield return null;

        onDone?.Invoke(SaveHandler.WriteSession(slot));
    }

    /// <summary>Host side: removes a player after getting their newest character, so it is saved as they leave.</summary>
    public static void RemovePlayerAfterSaving(ulong clientId)
    {
        NetworkPlayer host = Local;
        if (host == null || !host.IsServer) return;
        host.StartCoroutine(host.CollectThenRemove(clientId));
    }

    private IEnumerator CollectThenRemove(ulong clientId)
    {
        NetworkPlayer guest = GuestPlayers().Find(g => g.OwnerClientId == clientId);
        if (guest != null)
        {
            float requestedAt = Time.unscaledTime;
            guest.RequestCharacterClientRpc(guest.ToOwner());
            while (guest != null && guest.LastCharacterReceivedAt < requestedAt && Time.unscaledTime - requestedAt < SaveCollectTimeout)
                yield return null;
        }
        // Leaving saves the session with their character (see GameSession.HandleGuestLeft).
        if (RelayConnectionManager.Instance != null) RelayConnectionManager.Instance.RemovePlayer(clientId);
    }

    private static List<NetworkPlayer> GuestPlayers()
    {
        List<NetworkPlayer> guests = new();
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            if (client.ClientId != NetworkManager.ServerClientId && client.PlayerObject != null && client.PlayerObject.TryGetComponent(out NetworkPlayer player))
                guests.Add(player);
        return guests;
    }

    #endregion

    #region Chunked transfer

    private void SendChunks(string json, Action<uint, int, int, byte[]> send)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        int count = Mathf.Max(1, (bytes.Length + CharacterChunkSize - 1) / CharacterChunkSize);
        uint transferId = ++nextTransferId;
        for (int i = 0; i < count; i++)
        {
            int length = Mathf.Min(CharacterChunkSize, bytes.Length - i * CharacterChunkSize);
            byte[] chunk = new byte[Mathf.Max(0, length)];
            if (length > 0) Array.Copy(bytes, i * CharacterChunkSize, chunk, 0, length);
            send(transferId, i, count, chunk);
        }
    }

    private bool TryReceiveChunk(uint transferId, int index, int count, byte[] chunk, out string json)
    {
        json = null;
        if (count <= 0 || index < 0 || index >= count) return false;
        if (!incomingTransfers.TryGetValue(transferId, out byte[][] parts) || parts.Length != count)
        {
            parts = new byte[count][];
            incomingTransfers[transferId] = parts;
        }
        parts[index] = chunk ?? Array.Empty<byte>();
        foreach (byte[] part in parts) if (part == null) return false;

        incomingTransfers.Remove(transferId);
        int total = 0;
        foreach (byte[] part in parts) total += part.Length;
        byte[] bytes = new byte[total];
        int offset = 0;
        foreach (byte[] part in parts)
        {
            Array.Copy(part, 0, bytes, offset, part.Length);
            offset += part.Length;
        }
        json = Encoding.UTF8.GetString(bytes);
        return true;
    }

    #endregion
}
