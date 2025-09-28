using System.Collections.Generic;



public static class AlchemyInventory
{
    #region Class variables
    public static Dictionary<AlchemyLoot, int> ingredients { get; private set; } = new()
        {
            { AlchemyLoot.WretchedOrgans, 10 },
            { AlchemyLoot.FunctionalOrgans, 10 },
            { AlchemyLoot.HulkingOrgans, 10 },
            { AlchemyLoot.WeakCores, 10 },
            { AlchemyLoot.WorkingCore, 10 },
            { AlchemyLoot.PowerfulCore, 10 },
            { AlchemyLoot.HulkingCore, 10 },
            { AlchemyLoot.FaintEther, 10 },
            { AlchemyLoot.PureEther, 10 },
            { AlchemyLoot.IntenseEther,101 }
            };
    public static Dictionary<AlchemyLoot, int> ingredientValues { get; private set; } = new()
        {
            { AlchemyLoot.WretchedOrgans, 1 },
            { AlchemyLoot.FunctionalOrgans, 2 },
            { AlchemyLoot.HulkingOrgans, 4 },
            { AlchemyLoot.WeakCores, 1 },
            { AlchemyLoot.WorkingCore, 2 },
            { AlchemyLoot.PowerfulCore, 4 },
            { AlchemyLoot.HulkingCore, 6 },
            { AlchemyLoot.FaintEther, 1 },
            { AlchemyLoot.PureEther, 2 },
            { AlchemyLoot.IntenseEther, 4 }
            };
    public static Dictionary<AlchemyLoot, int> AlchemyLootPowerValues { get; private set; } = new()
        {
            { AlchemyLoot.WretchedOrgans, 5 },
            { AlchemyLoot.FunctionalOrgans, 10 },
            { AlchemyLoot.HulkingOrgans, 20 },
            { AlchemyLoot.WeakCores, 10 },
            { AlchemyLoot.WorkingCore, 20 },
            { AlchemyLoot.PowerfulCore, 30 },
            { AlchemyLoot.HulkingCore, 60 },
            { AlchemyLoot.FaintEther, 10 },
            { AlchemyLoot.PureEther, 20 },
            { AlchemyLoot.IntenseEther, 50 }
            };

    public static Dictionary<Element, int> knowledgeDict = new()
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
        if (element != Element.None)
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
    #region Check suffient resource

    public static bool HasSufficientCorePower(Ability ability, Dictionary<AlchemyLoot, int> coreKvp)
    {
        int corePower = GetCorePowerResource(coreKvp);
        if (Ability.GetCoreCraftingCost(ability) <= corePower)
        {
            ExpendIngredients(coreKvp);
            return true;
        }
        else return false;
    }

    public static int GetCorePowerResource(Dictionary<AlchemyLoot, int> coreKvp)
    {
        int corePower = 0;
        foreach (KeyValuePair<AlchemyLoot, int> kvp in coreKvp)
        {
            corePower += AlchemyLootPowerValues[kvp.Key] * kvp.Value;
        }
        return corePower;
    }

    #endregion
}


