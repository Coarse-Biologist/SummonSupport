using Alchemy;
using UnityEngine;

public class LootHandler : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        ILootInterface lootInterfaceInstance = collision.gameObject.GetComponent<ILootInterface>();
        if (lootInterfaceInstance != null)
        {
            lootInterfaceInstance.Loot();
        }
    }

}
