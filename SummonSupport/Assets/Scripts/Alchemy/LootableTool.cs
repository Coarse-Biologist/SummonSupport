using UnityEngine;
using Alchemy;

public class LootableTool : MonoBehaviour, ILootInterface
{
    [SerializeField] public AlchemyTools tool;

    public void Loot()
    {
        AlchemyInventory.GainTool(tool);
        Destroy(this.gameObject);
    }
}
