using UnityEngine;

public class LootableTool : MonoBehaviour, I_LootInterface
{
    [SerializeField] public AlchemyTool tool;

    public void Loot()
    {
        AlchemyInventory.GainTool(tool);
        Destroy(this.gameObject);
    }
}
