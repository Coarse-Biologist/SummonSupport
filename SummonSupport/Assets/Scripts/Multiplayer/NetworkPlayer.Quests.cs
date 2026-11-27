using Quest;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Sharing quests between everyone in the game (see QuestHandler).
/// Clients send the host the progress they make; the host sends everyone the quests it starts and completes,
/// the counts it reaches, and a joining player the whole state.
/// </summary>
public partial class NetworkPlayer
{
    #region Client to host

    /// <summary>Client side: progress made on this machine, for the host to count.</summary>
    public static void ReportQuestProgress(RepeatableAccomplishments accomplishment, int value)
    {
        NetworkPlayer local = PlayerStats.Instance != null ? PlayerStats.Instance.GetComponent<NetworkPlayer>() : null;
        if (local == null || !local.IsSpawned || !local.IsOwner) return;
        local.ReportQuestProgressServerRpc(accomplishment, value);
    }

    [ServerRpc]
    private void ReportQuestProgressServerRpc(RepeatableAccomplishments accomplishment, int value)
    {
        if (QuestHandler.Instance != null) QuestHandler.Instance.IncrementIntQuest(accomplishment, value);
    }

    #endregion

    #region Host to everyone

    // The host's own player sends the host's messages: it is spawned for as long as the game runs.
    private static NetworkPlayer HostSender()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (!NetworkSession.IsOnline || networkManager == null || !networkManager.IsServer || networkManager.LocalClient == null) return null;
        NetworkObject playerObject = networkManager.LocalClient.PlayerObject;
        return playerObject != null && playerObject.IsSpawned ? playerObject.GetComponent<NetworkPlayer>() : null;
    }

    public static void BroadcastQuestStarted(Quest_SO quest)
    {
        NetworkPlayer sender = HostSender();
        if (sender != null && quest != null) sender.QuestStartedClientRpc(quest.name);
    }

    public static void BroadcastQuestCompleted(Quest_SO quest)
    {
        NetworkPlayer sender = HostSender();
        if (sender != null && quest != null) sender.QuestCompletedClientRpc(quest.name);
    }

    public static void BroadcastQuestCount(RepeatableAccomplishments accomplishment, int count)
    {
        NetworkPlayer sender = HostSender();
        if (sender != null) sender.QuestCountClientRpc(accomplishment, count);
    }

    /// <summary>Host side: sends every client the whole quest state, e.g. after going back to a save.</summary>
    public static void BroadcastQuestProgress()
    {
        NetworkPlayer sender = HostSender();
        if (sender != null) sender.QuestProgressClientRpc(JsonUtility.ToJson(QuestHandler.BuildProgress()));
    }

    /// <summary>Host side: brings a joining player's quests up to date.</summary>
    private void SendQuestProgressToOwner()
    {
        QuestProgressClientRpc(JsonUtility.ToJson(QuestHandler.BuildProgress()), ToOwner());
    }

    // The host has already applied these itself, so each one is for the clients only.

    [ClientRpc]
    private void QuestStartedClientRpc(string questName)
    {
        if (!IsServer) QuestHandler.ApplyQuestStarted(questName);
    }

    [ClientRpc]
    private void QuestCompletedClientRpc(string questName)
    {
        if (!IsServer) QuestHandler.ApplyQuestCompleted(questName);
    }

    [ClientRpc]
    private void QuestCountClientRpc(RepeatableAccomplishments accomplishment, int count)
    {
        if (!IsServer) QuestHandler.ApplyQuestCount(accomplishment, count);
    }

    [ClientRpc]
    private void QuestProgressClientRpc(string progressJson, ClientRpcParams rpcParams = default)
    {
        if (!IsServer) QuestHandler.ApplyProgress(JsonUtility.FromJson<QuestProgressData>(progressJson));
    }

    #endregion
}
