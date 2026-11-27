using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayConnectionManager : MonoBehaviour
{
    public static RelayConnectionManager Instance { get; private set; }

    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private string connectionType = "dtls";

    public string JoinCode { get; private set; }

    // The join code of the session this machine is in right now, or null when offline.
    // A client returns the code it joined with, which others can also use.
    public string ActiveJoinCode =>
        NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !string.IsNullOrEmpty(JoinCode) ? JoinCode : null;
    public bool ServicesReady { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
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
        await InitializeServicesAsync();

        // maxConnections excludes the host, which occupies one of the maxPlayers slots.
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
        JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(allocation, connectionType));

        NetworkManager.Singleton.StartHost();
    }

    public async Task JoinAsync(string joinCode)
    {
        await InitializeServicesAsync();

        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        JoinCode = joinCode;

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(joinAllocation, connectionType));

        NetworkManager.Singleton.StartClient();
    }

    public void Disconnect()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();
        JoinCode = null;
    }
}
