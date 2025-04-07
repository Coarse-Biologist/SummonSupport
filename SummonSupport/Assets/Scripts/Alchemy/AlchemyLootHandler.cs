using Alchemy;
using UnityEngine;

public class AlchemyLootHandler : MonoBehaviour, ILootInterface
{
    [SerializeField] AlchemyTools tool;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Loot();
        }
    }

    public void Loot()
    {
        AlchemyInventory.GainTool(tool);
    }
}
