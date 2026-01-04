using UnityEngine;

public class LootHandler : MonoBehaviour
{
    void OnTriggerEnter(Collider collision)
    {
        I_LootInterface lootInterfaceInstance = collision.gameObject.GetComponent<I_LootInterface>();
        if (lootInterfaceInstance != null)
        {
            lootInterfaceInstance.Loot();
        }
    }

}
