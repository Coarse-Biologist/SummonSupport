using UnityEngine;

public class LootableTool : MonoBehaviour, ILootInterface
{
    [SerializeField] public AlchemyTool tool;

    public void Loot()
    {
        AlchemyInventory.GainTool(tool);
        Destroy(this.gameObject);
    }
}
