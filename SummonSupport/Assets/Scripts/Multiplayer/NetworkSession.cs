using Unity.Netcode;

public static class NetworkSession
{
    public static bool IsOnline => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    public static bool IsAuthority => !IsOnline || NetworkManager.Singleton.IsServer;
}
