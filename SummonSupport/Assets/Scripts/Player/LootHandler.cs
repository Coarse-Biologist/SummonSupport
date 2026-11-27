using UnityEngine;

public class LootHandler : MonoBehaviour
{
    private NetworkPlayer networkPlayer;

    void Awake()
    {
        networkPlayer = GetComponent<NetworkPlayer>();
    }

    void OnTriggerEnter(Collider collision)
    {
        I_LootInterface lootInterfaceInstance = collision.gameObject.GetComponent<I_LootInterface>();
        if (lootInterfaceInstance == null) return;

        bool online = networkPlayer != null && networkPlayer.IsSpawned;
        if (!online)
        {
            lootInterfaceInstance.Loot();
            return;
        }

        // Online, each machine only picks up loot for its own player; copies of other players walk through it.
        if (!networkPlayer.IsOwner) return;

        // Dropped loot is shared: ask the host, and it goes into everyone's inventory (see LootSpawnHandler).
        if (LootSpawnHandler.Instance != null && LootSpawnHandler.Instance.TryGetLootId(collision.gameObject, out uint lootId))
            networkPlayer.RequestLoot(lootId);
        else
            lootInterfaceInstance.Loot();
    }

}
