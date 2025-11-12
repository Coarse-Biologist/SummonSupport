using UnityEngine;

public class LootableTool : MonoBehaviour, I_LootInterface
{
    [SerializeField] public AlchemyTool tool;

    public void Loot()
    {
        AlchemyInventory.GainTool(tool);
        InteractCanvasHandler.Instance.DisplayGoldenLetters($"{tool} learned!", 1f);

        Destroy(this.gameObject);
    }
}
