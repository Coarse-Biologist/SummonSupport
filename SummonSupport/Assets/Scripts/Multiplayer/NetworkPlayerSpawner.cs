using System.Collections.Generic;
using SummonSupportEvents;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkPlayerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private string gameplaySceneName = "Map1";

    private readonly Dictionary<ulong, NetworkObject> players = new();
    private NetworkManager networkManager;
    private bool gameplayLoaded;
    private NetworkSceneManager sceneManager;

    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        networkManager.OnServerStarted += OnServerStarted;
        networkManager.OnServerStopped += OnServerStopped;
        EventDeclarer.EnemyDefeated?.AddListener(OnEnemyDefeated);
    }

    private void OnDestroy()
    {
        EventDeclarer.EnemyDefeated?.RemoveListener(OnEnemyDefeated);
        if (networkManager == null) return;
        networkManager.OnServerStarted -= OnServerStarted;
        networkManager.OnServerStopped -= OnServerStopped;
        Unsubscribe();
    }

    // This lives on the NetworkManager, which outlasts each session, and Netcode makes a new SceneManager for
    // every session: so subscribe again, and forget the last session's players, each time a session starts.
    private void OnServerStarted()
    {
        Unsubscribe();
        players.Clear();
        gameplayLoaded = false;
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        sceneManager = networkManager.SceneManager;
        sceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
    }

    private void OnServerStopped(bool wasHost)
    {
        Unsubscribe();
        players.Clear();
        gameplayLoaded = false;
    }

    private void Unsubscribe()
    {
        networkManager.OnClientConnectedCallback -= OnClientConnected;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        if (sceneManager != null) sceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        sceneManager = null;
    }

    private void OnEnemyDefeated(EnemyStats enemy)
    {
        if (networkManager == null || !networkManager.IsListening || !networkManager.IsServer) return;

        ulong killer = enemy.GetComponent<NetworkEnemy>() is NetworkEnemy networkEnemy ? networkEnemy.LastAttackerClientId : NetworkManager.ServerClientId;
        float baseXp = enemy.XP_OnDeath;

        foreach (ulong clientId in networkManager.ConnectedClientsIds)
        {
            int amount = Mathf.RoundToInt(clientId == killer ? baseXp * 1.5f : baseXp);
            NetworkObject playerObject = networkManager.SpawnManager.GetPlayerNetworkObject(clientId);
            if (playerObject == null) continue;

            if (clientId == NetworkManager.ServerClientId) playerObject.GetComponent<PlayerStats>().GainXP(amount);
            else playerObject.GetComponent<NetworkPlayer>().GrantXp(amount, clientId);
        }
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        gameplayLoaded = sceneName == gameplaySceneName;
        if (!gameplayLoaded) return;

        foreach (ulong clientId in networkManager.ConnectedClientsIds)
            SpawnPlayer(clientId);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!networkManager.IsServer || !gameplayLoaded) return;
        SpawnPlayer(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        players.Remove(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (players.ContainsKey(clientId) || networkManager.SpawnManager.GetPlayerNetworkObject(clientId) != null)
        {
            Debug.Log($"[Spawner] client {clientId} already has a player; skipping.");
            return;
        }
        Debug.Log($"[Spawner] spawning player for client {clientId}.");

        PlayerSpawnPoint[] spawnPoints = FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);
        NetworkObject player = Instantiate(playerPrefab);
        if (spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[players.Count % spawnPoints.Length].transform;
            player.transform.SetPositionAndRotation(point.position, point.rotation);
        }
        else Debug.LogWarning("No PlayerSpawnPoint in the gameplay scene; spawning player at the prefab origin.");

        player.SpawnAsPlayerObject(clientId);
        players[clientId] = player;
    }
}
