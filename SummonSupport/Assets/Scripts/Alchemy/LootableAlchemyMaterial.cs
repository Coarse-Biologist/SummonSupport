using UnityEngine;
using System.Collections.Generic;

public class LootableAlchemyMaterial : MonoBehaviour, I_LootInterface
{
    [SerializeField] public AlchemyLoot alchemyMaterial;

    public void Loot()
    {
        AlchemyInventory.AlterIngredientNum(alchemyMaterial, 1);
        Destroy(this.gameObject);
    }
}
