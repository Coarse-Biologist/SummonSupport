using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The game chosen on the start screen: its save slot, its difficulty and the save it continues.
/// The host (or the single player) also keeps the newest copy of every other player's character here,
/// so it can write them to the slot whenever it saves, including when someone leaves (see SaveHandler).
/// </summary>
public static class GameSession
{
    public const int NoSlot = 0;

    public static int ActiveSlot { get; private set; } = NoSlot;
    public static Difficulty? Difficulty { get; private set; }

    // The host's own saved character, loaded once its player exists.
    private static SaveData pendingHostCharacter;
    // The loaded save's quest progress, taken by the level's QuestHandler when it starts.
    private static QuestProgressData pendingQuestProgress;
    // Every other player's character, by player id: from the loaded save, then the newest copy each player sent.
    private static readonly Dictionary<string, PlayerSaveRecord> guestRecords = new();
    private static readonly Dictionary<ulong, string> playerIdsByClient = new();

    public static void Begin(int slot, Difficulty difficulty, SessionSaveData loadedSave)
    {
        End();
        ActiveSlot = slot;
        Difficulty = difficulty;
        if (loadedSave == null) return;

        pendingHostCharacter = loadedSave.HostRecord?.data;
        pendingQuestProgress = loadedSave.quests;
        foreach (PlayerSaveRecord record in loadedSave.players)
            if (!record.isHost && !string.IsNullOrEmpty(record.playerId)) guestRecords[record.playerId] = record;
    }

    public static void End()
    {
        ActiveSlot = NoSlot;
        Difficulty = null;
        pendingHostCharacter = null;
        pendingQuestProgress = null;
        guestRecords.Clear();
        playerIdsByClient.Clear();
        QuestHandler.ResetProgress(); // quests belong to one game, so the next one starts without them
    }

    public static void SetActiveSlot(int slot)
    {
        ActiveSlot = slot;
    }

    /// <summary>The host's saved character, if any; handed out once.</summary>
    public static SaveData TakeHostCharacter()
    {
        SaveData character = pendingHostCharacter;
        pendingHostCharacter = null;
        return character;
    }

    /// <summary>The loaded save's quest progress, if any; handed out once.</summary>
    public static QuestProgressData TakeQuestProgress()
    {
        QuestProgressData progress = pendingQuestProgress;
        pendingQuestProgress = null;
        return progress;
    }

    public static IEnumerable<PlayerSaveRecord> GuestRecords => guestRecords.Values;

    #region Host side: players in the session

    /// <summary>Remembers which player a client is, and returns their saved character if this session has one.</summary>
    public static PlayerSaveRecord RegisterPlayer(ulong clientId, string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) playerId = $"client-{clientId}";
        if (playerIdsByClient.TryGetValue(clientId, out string known)) playerId = known; // e.g. respawned after a restart
        // Two copies of the game on one computer can share an id; keep their characters apart.
        else if (playerIdsByClient.ContainsValue(playerId)) playerId = $"{playerId}-{clientId}";
        playerIdsByClient[clientId] = playerId;
        return guestRecords.TryGetValue(playerId, out PlayerSaveRecord record) ? record : null;
    }

    public static bool IsRegistered(ulong clientId) => playerIdsByClient.ContainsKey(clientId);

    /// <summary>Keeps the newest copy of a player's character, which they built on their own machine.</summary>
    public static void StoreCharacter(ulong clientId, string playerName, SaveData character)
    {
        if (character == null || !playerIdsByClient.TryGetValue(clientId, out string playerId)) return;
        guestRecords[playerId] = new PlayerSaveRecord
        {
            playerId = playerId,
            playerName = playerName,
            isHost = false,
            saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            data = character
        };
    }

    /// <summary>
    /// Host side: a player left, was removed or dropped. Their character stays in the session, and the slot is
    /// saved with it, so they can carry on later.
    /// </summary>
    public static void HandleGuestLeft(ulong clientId)
    {
        if (!playerIdsByClient.TryGetValue(clientId, out string playerId)) return;
        playerIdsByClient.Remove(clientId);
        if (!guestRecords.ContainsKey(playerId)) return;
        if (ActiveSlot == NoSlot)
        {
            Debug.Log("A player left, but this game has no save slot yet; save from the pause menu to keep their character.");
            return;
        }
        SaveHandler.WriteSession(ActiveSlot);
    }

    /// <summary>
    /// Restarting goes back to the slot's last save: the host's character and every other player's are
    /// read from it again. Players stay matched to their clients.
    /// </summary>
    public static void ReloadFromSlot()
    {
        SessionSaveData save = ActiveSlot != NoSlot ? SaveHandler.ReadSlot(ActiveSlot) : null;
        pendingHostCharacter = save?.HostRecord?.data;
        pendingQuestProgress = save?.quests;
        QuestHandler.ResetProgress();
        guestRecords.Clear();
        if (save == null) return;
        foreach (PlayerSaveRecord record in save.players)
            if (!record.isHost && !string.IsNullOrEmpty(record.playerId)) guestRecords[record.playerId] = record;
    }

    #endregion
}
