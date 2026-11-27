using Unity.Netcode;

public static class NetworkSession
{
    public static bool IsOnline => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    public static bool IsAuthority => !IsOnline || NetworkManager.Singleton.IsServer;

    public static bool IsHost => IsOnline && NetworkManager.Singleton.IsHost;

    /// <summary>Online as a client: the host decides things like saving, loading and restarting.</summary>
    public static bool IsClientOnly => IsOnline && !NetworkManager.Singleton.IsServer;
}
