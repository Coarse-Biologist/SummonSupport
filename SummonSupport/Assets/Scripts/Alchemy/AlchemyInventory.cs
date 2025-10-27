using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;



public static class AlchemyInventory
{
    #region Class variables
    public static Dictionary<AlchemyLoot, int> ingredients { get; private set; } = new()
        {
            { AlchemyLoot.WretchedOrgans, 10 },
            { AlchemyLoot.FunctionalOrgans, 10 },
            { AlchemyLoot.HulkingOrgans, 10 },
            { AlchemyLoot.WeakCore, 10 },
            { AlchemyLoot.SolidCore, 10 },
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
            { AlchemyLoot.WeakCore, 1 },
            { AlchemyLoot.SolidCore, 2 },
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
            { AlchemyLoot.WeakCore, 10 },
            { AlchemyLoot.SolidCore, 20 },
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
    public static int GetElementalKnowledge(Element element)
    {
        if (element != Element.None && knowledgeDict.TryGetValue(element, out int value))
        {
            return value;
        }
        else return 0;
    }
    public static void AlterIngredientNum(AlchemyLoot newIngredient, int amount)
    {
        //UnityEngine.Debug.Log($"Adding {amount} {newIngredient}");
        ingredients[newIngredient] += amount;
    }
    public static void ExpendIngredients(Dictionary<AlchemyLoot, int> usedIngredients)
    {
        foreach (KeyValuePair<AlchemyLoot, int> kvp in usedIngredients)
        {
            if (ingredients[kvp.Key] > 0) AlterIngredientNum(kvp.Key, -kvp.Value);
        }
    }
    public static void GainTool(AlchemyTool tool)
    {
        if (!KnownTools.Contains(tool))
        {
            KnownTools.Add(tool);
        }
        //else Logging.Error($"The tool {tool} is already known");
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
            if (kvp.Key.ToString().Contains("Core")) corePower += AlchemyLootPowerValues[kvp.Key] * kvp.Value;
        }
        return corePower;
    }

    public static (bool bought, int price) BuyCraftingPowerWithCores(int requiredCraftingPower) // returns true if trade was possible/ successful - false otherwise
    {
        Dictionary<AlchemyLoot, int> CoreDict = ingredients.Where(g => g.ToString().Contains("Core")).ToDictionary(g => g.Key, g => g.Value);
        int currentPotentialPower = GetCorePowerResource(CoreDict);
        (bool sufficient, int currentExpenditure) continueTuple = new();
        if (currentPotentialPower < requiredCraftingPower) return (false, 0);
        else
        {
            continueTuple = ExpendCoresWhile(CoreDict, AlchemyLoot.WeakCore, 0, requiredCraftingPower);
            if (!continueTuple.sufficient)
                continueTuple = ExpendCoresWhile(CoreDict, AlchemyLoot.SolidCore, continueTuple.currentExpenditure, requiredCraftingPower);
            if (!continueTuple.sufficient)
                continueTuple = ExpendCoresWhile(CoreDict, AlchemyLoot.PowerfulCore, continueTuple.currentExpenditure, requiredCraftingPower);
            if (!continueTuple.sufficient)
                continueTuple = ExpendCoresWhile(CoreDict, AlchemyLoot.HulkingCore, continueTuple.currentExpenditure, requiredCraftingPower);

        }
        return (true, continueTuple.currentExpenditure);
    }

    private static (bool sufficient, int currentExpenditure) ExpendCoresWhile(Dictionary<AlchemyLoot, int> CoreDict, AlchemyLoot coreType, int currentlySpent, int goalNum)
    {
        int recursions = 0;
        int maxRecursions = 100;
        while (CoreDict.TryGetValue(coreType, out int num) && num > 0 && currentlySpent < goalNum)
        {
            CoreDict[coreType] = num - 1;
            AlterIngredientNum(coreType, -1);
            currentlySpent += AlchemyLootPowerValues[coreType];
            recursions += 1;
            if (recursions >= maxRecursions)
            {
                throw new System.Exception("GG Brueder, cant even make a simple chain of nested 'while loops'?");
            }
        }
        return (currentlySpent >= goalNum, currentlySpent);
    }
    #endregion

    public static string GetAlchemyLootString(AlchemyLoot lootString)
    {
        return System.Text.RegularExpressions.Regex.Replace(lootString.ToString(), "(?<!^)([A-Z])", " $1");
    }
}
