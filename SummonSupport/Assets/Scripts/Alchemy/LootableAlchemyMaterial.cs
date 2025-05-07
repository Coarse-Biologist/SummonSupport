using UnityEngine;
using System.Collections.Generic;

public class LootableAlchemyMaterial : MonoBehaviour, I_LootInterface
{
    [SerializeField] public AlchemyLoot alchemyMaterial;

    public void Loot()
    {
        AlchemyInventory.AlterIngredientNum(new List<AlchemyLoot> { alchemyMaterial }, 1);
        Destroy(this.gameObject);
    }
}
