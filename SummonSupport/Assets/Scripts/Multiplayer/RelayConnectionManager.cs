using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RelayConnectionManager : MonoBehaviour
{
    public static RelayConnectionManager Instance { get; private set; }

    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private string connectionType = "dtls";
    [SerializeField] private string startSceneName = "StartScreen";

    public string JoinCode { get; private set; }

    // The join code of the session this machine is in right now, or null when offline.
    // A client returns the code it joined with, which others can also use.
    public string ActiveJoinCode =>
        NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !string.IsNullOrEmpty(JoinCode) ? JoinCode : null;
    public bool ServicesReady { get; private set; }

    /// <summary>This player's Unity Authentication id, which stays the same between sessions on this device.</summary>
    public string LocalPlayerId => ServicesReady && AuthenticationService.Instance.IsSignedIn ? AuthenticationService.Instance.PlayerId : null;

    /// <summary>Why this client was last dropped from a session (e.g. removed by the host); the start screen shows it once.</summary>
    public string LastDisconnectReason { get; set; }

    private bool leaving;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Lives on the NetworkManager's GameObject, which persists across scenes.
        if (TryGetComponent(out NetworkManager networkManager)) networkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        if (TryGetComponent(out NetworkManager networkManager)) networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    // A client that loses the host (removed, host quit, connection dropped) goes back to the start screen.
    private void OnClientDisconnected(ulong clientId)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (leaving) return;
        if (networkManager.IsServer)
        {
            if (clientId != NetworkManager.ServerClientId) GameSession.HandleGuestLeft(clientId);
            return;
        }
        if (clientId != networkManager.LocalClientId && clientId != NetworkManager.ServerClientId) return;

        LastDisconnectReason = string.IsNullOrEmpty(networkManager.DisconnectReason) ? "Lost connection to the host." : networkManager.DisconnectReason;
        JoinCode = null;
        if (SceneManager.GetActiveScene().name == startSceneName) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(startSceneName);
    }

    /// <summary>Host only: disconnects a player and tells them why.</summary>
    public void RemovePlayer(ulong clientId)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null || !networkManager.IsServer || clientId == NetworkManager.ServerClientId) return;
        networkManager.DisconnectClient(clientId, "You were removed from the session by the host.");
    }

    public async Task InitializeServicesAsync()
    {
        if (ServicesReady) return;
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        ServicesReady = true;
    }

    public async Task HostAsync()
    {
        leaving = false;
        await InitializeServicesAsync();

        // maxConnections excludes the host, which occupies one of the maxPlayers slots.
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
        JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        await WaitForShutdown();
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(allocation, connectionType));

        if (!NetworkManager.Singleton.StartHost()) throw new System.InvalidOperationException("Netcode could not start the host.");
    }

    public async Task JoinAsync(string joinCode)
    {
        leaving = false;
        await InitializeServicesAsync();

        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        JoinCode = joinCode;

        await WaitForShutdown();
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(joinAllocation, connectionType));

        if (!NetworkManager.Singleton.StartClient()) throw new System.InvalidOperationException("Netcode could not start the client.");
    }

    // A session that was just left finishes shutting down a frame later; a new one can't start before that.
    private static async Task WaitForShutdown()
    {
        while (NetworkManager.Singleton != null && NetworkManager.Singleton.ShutdownInProgress) await Task.Yield();
    }

    public void Disconnect()
    {
        leaving = true;
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();
        JoinCode = null;
    }
}
