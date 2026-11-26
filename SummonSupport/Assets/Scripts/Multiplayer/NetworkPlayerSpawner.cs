using System.Collections.Generic;
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
    private bool subscribed;

    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        networkManager.OnServerStarted += OnServerStarted;
    }

    private void OnDestroy()
    {
        if (networkManager != null) networkManager.OnServerStarted -= OnServerStarted;
    }

    private void OnServerStarted()
    {
        if (subscribed) return;
        subscribed = true;
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        networkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
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
        if (players.ContainsKey(clientId)) return;

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
