using UnityEngine;
using System.Collections.Generic;

public class LootableAlchemyMaterial : MonoBehaviour, I_LootInterface
{
    [SerializeField] public AlchemyLoot alchemyMaterial;
    [SerializeField] public Element Element = Element.None;


    public void Loot()
    {
        AlchemyInventory.AlterIngredientNum(alchemyMaterial, 1);
        if (alchemyMaterial.ToString().Contains("Ether"))
        {
            if (AlchemyHandler.AlchemyLootValueDict.TryGetValue(alchemyMaterial, out int num))
            {
                int knowledgeGain = (int)(num * AlchemyHandler.knowledgeGainRate);
                AlchemyInventory.IncemementElementalKnowledge(Element, (int)(num * AlchemyHandler.knowledgeGainRate));
                Debug.Log($"Knowledge gain = {knowledgeGain} for element {Element}");
            }
        }
        Destroy(this.gameObject);
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
