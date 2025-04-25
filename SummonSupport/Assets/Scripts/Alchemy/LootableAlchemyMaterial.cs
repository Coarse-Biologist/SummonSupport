using UnityEngine;

public class LootableAlchemyMaterial : MonoBehaviour, ILootInterface
{
    [SerializeField] public AlchemyLoot alchemyMaterial;

    public void Loot()
    {
        AlchemyInventory.AlterIngredientNum(alchemyMaterial, 1);
        Destroy(this.gameObject);
    }
}
