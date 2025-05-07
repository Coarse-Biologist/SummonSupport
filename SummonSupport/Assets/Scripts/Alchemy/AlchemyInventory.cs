using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;


public static class AlchemyInventory
{
    #region Class variables
    public static Dictionary<AlchemyLoot, int> ingredients { get; private set; } = new Dictionary<AlchemyLoot, int>
        {
            { AlchemyLoot.WretchedOrgans, 2 },
            { AlchemyLoot.FunctionalOrgans, 2 },
            { AlchemyLoot.HulkingOrgans, 2 },
            { AlchemyLoot.BrokenCores, 2 },
            { AlchemyLoot.WorkingCore, 1 },
            { AlchemyLoot.PowerfulCore, 0 },
            { AlchemyLoot.HulkingCore, 0 },
            { AlchemyLoot.FaintEther, 1 },
            { AlchemyLoot.PureEther, 2 },
            { AlchemyLoot.IntenseEther, 1 }
            };
    public static Dictionary<AlchemyLoot, int> ingredientValues { get; private set; } = new Dictionary<AlchemyLoot, int>
        {
            { AlchemyLoot.WretchedOrgans, 1 },
            { AlchemyLoot.FunctionalOrgans, 2 },
            { AlchemyLoot.HulkingOrgans, 4 },
            { AlchemyLoot.BrokenCores, 1 },
            { AlchemyLoot.WorkingCore, 2 },
            { AlchemyLoot.PowerfulCore, 4 },
            { AlchemyLoot.HulkingCore, 6 },
            { AlchemyLoot.FaintEther, 1 },
            { AlchemyLoot.PureEther, 2 },
            { AlchemyLoot.IntenseEther, 4 }
            };

    public static Dictionary<Element, int> knowledgeDict = new Dictionary<Element, int>
            {
                { Element.Cold, 0 },
                { Element.Water, 0 },
                { Element.Earth, 0 },
                { Element.Heat, 0 },
                { Element.Air, 0 },
                { Element.Electricity, 0 },
                { Element.Poison, 0 },
                { Element.Acid, 0 },
                { Element.Bacteria, 0 },
                { Element.Fungi, 0 },
                { Element.Plant, 0 },
                { Element.Virus, 0 },
                { Element.Radiation, 0 },
                { Element.Light, 0 },
                { Element.Psychic, 0 }
            };

    public static List<AlchemyTool> KnownTools = new List<AlchemyTool>();

    #endregion

    #region Set Dict values
    public static void IncemementElementalKnowledge(Element element, int amount)
    {
        knowledgeDict[element] += amount;
    }
    public static void AlterIngredientNum(List<AlchemyLoot> newIngredients, int amount)
    {
        foreach (AlchemyLoot ingredient in newIngredients)
        {
            ingredients[ingredient] += amount;
        }
    }
    public static void ExpendIngredients(Dictionary<AlchemyLoot, int> usedIngredients)
    {
        foreach (KeyValuePair<AlchemyLoot, int> kvp in usedIngredients)
        {
            if (ingredients[kvp.Key] > 0) AlterIngredientNum(new List<AlchemyLoot> { kvp.Key }, -kvp.Value);
        }
    }
    public static void GainTool(AlchemyTool tool)
    {
        if (!KnownTools.Contains(tool))
        {
            KnownTools.Add(tool);
        }
        else Logging.Error($"The tool {tool} is already known");
    }
    #endregion

    #region Check dict values
    public static bool GetSufficentIngredients(AlchemyLoot ingredient, int amount)
    {
        bool sufficient = false;
        if (ingredients[ingredient] >= amount) sufficient = true;

        return sufficient;
    }
    #endregion


}


