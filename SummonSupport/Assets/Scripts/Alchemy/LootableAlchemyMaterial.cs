using UnityEngine;
using System.Collections.Generic;

public class LootableAlchemyMaterial : MonoBehaviour, I_LootInterface
{
    [SerializeField] public AlchemyLoot alchemyMaterial;
    [SerializeField] public Element Element = Element.None;


    public void Loot()
    {
        AlchemyInventory.AlterIngredientNum(alchemyMaterial, 1);
        FloatingInfoHandler.Instance.DisplayGoldenLetters($"+1 {alchemyMaterial}!", 1.5f);
        if (alchemyMaterial.ToString().Contains("Ether"))
        {
            if (AlchemyHandler.AlchemyLootValueDict.TryGetValue(alchemyMaterial, out int num))
            {
                AlchemyInventory.IncemementElementalKnowledge(Element, (int)(num * AlchemyHandler.Instance.KnowledgeGainRate));
            }
        }
        Destroy(gameObject);
    }

    public void SetAlchemyMaterial(AlchemyLoot assignedLoot)
    {
        alchemyMaterial = assignedLoot;
    }
    public void SetElement(Element assignedElement)
    {
        Element = assignedElement;
    }

}
