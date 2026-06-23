using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quest;
using SummonSupportEvents;
using Unity.Entities.UniversalDelegates;
using UnityEditor;



public static class AlchemyInventory
{


    #region Class variables
    public static Dictionary<CraftingPotential, int> AvailableCraftingPotential { get; private set; } = new()
    {
        {CraftingPotential.OrganMass, 100 },
        {CraftingPotential.CorePower, 100 },
        {CraftingPotential.EtherDensity, 100 },

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
                { Element.Plant, 100 },
                { Element.Virus, 0 },
                { Element.Radiation, 0 },
                { Element.Light, 0 },
                { Element.Psychic, 0 }
            };

    public static List<AlchemyTool> KnownTools = new();

    #endregion

    #region Set Dict values
    public static void IncemementElementalKnowledge(Element element, int amount)
    {
        if (element != Element.None)
        {
            knowledgeDict[element] += amount;
            UnityEngine.Debug.Log($"element {element} kniweledge gained: {amount}");
        }
    }
    public static int GetElementalKnowledge(Element element)
    {
        if (element != Element.None && knowledgeDict.TryGetValue(element, out int value))
        {
            return value;
        }
        else return 0;
    }
    public static int GainKnowledge(List<Element> elementsList, Dictionary<CraftingPotential, int> usedPotential)
    {
        int total = 0;
        foreach (KeyValuePair<CraftingPotential, int> kvp in usedPotential)
        {
            int strengthAndAmount = kvp.Value; // amount of potential used
            strengthAndAmount = (int)(strengthAndAmount * AlchemyHandler.KnowledgeGainRate); //
            total += strengthAndAmount;

            foreach (Element element in elementsList)
            {
                IncemementElementalKnowledge(element, strengthAndAmount);
            }
        }

        return total;
    }
    public static void ConvertIngredientsToPotential(AlchemyLoot ingredient, int numberOfIngredients = 1)
    {
        // #TODO i hate this conditional business
        float modifier = 1 + KnownTools.Count / 20;
        RepeatableAccomplishments type = RepeatableAccomplishments.OrgansCollected;
        if (ingredientValues.TryGetValue(ingredient, out int value))
        {
            value *= numberOfIngredients;
            value *= (int)modifier;
            if (ingredient.ToString().Contains("Organ"))
            {
                type = RepeatableAccomplishments.OrgansCollected;

                AvailableCraftingPotential[CraftingPotential.OrganMass] += value;
            }
            else if (ingredient.ToString().Contains("Core"))
            {
                type = RepeatableAccomplishments.CoresCollected;

                AvailableCraftingPotential[CraftingPotential.CorePower] += value;
            }
            else if (ingredient.ToString().Contains("Ether"))
            {
                type = RepeatableAccomplishments.EtherCollected;
                AvailableCraftingPotential[CraftingPotential.EtherDensity] += value;
            }
            EventDeclarer.RepeatableQuestCompleted.Invoke(type, 1);
        }
    }

    public static void AlterCraftingPotential(CraftingPotential potential, int amount)
    {
        //UnityEngine.Debug.Log($"Adding {amount} {newIngredient}");
        AvailableCraftingPotential[potential] += amount;
    }

    public static void ExpendCraftingPotential(Dictionary<CraftingPotential, int> usedPotential)
    {
        foreach (KeyValuePair<CraftingPotential, int> kvp in usedPotential)
        {
            if (AvailableCraftingPotential[kvp.Key] > 0) AlterCraftingPotential(kvp.Key, -kvp.Value);
        }
    }
    public static void ExpendCraftingPotential(CraftingPotential potential, int value)
    {
        if (AvailableCraftingPotential[potential] > value) AlterCraftingPotential(potential, value);
        else throw new System.Exception("You are trying to spend more crafting potential than you have available.");
    }
    public static void GainTool(AlchemyTool tool)
    {
        if (!KnownTools.Contains(tool))
        {
            KnownTools.Add(tool);
            AlchemyHandler.ModifyToolBasedScaling(KnownTools.Count);
        }
        //else Logging.Error($"The tool {tool} is already known");
    }
    #endregion

    #region Check dict values

    #endregion
    #region Check suffient resource



    public static int GetCraftingPotential(CraftingPotential potentialType)
    {
        return AvailableCraftingPotential[potentialType];
    }

    public static void SetCraftingPotential(CraftingPotential potentialType, int value)
    {
        AvailableCraftingPotential[potentialType] = value;
    }
    #endregion
    public static void SetElementalKnowledge(Element element, int value)
    {
        knowledgeDict[element] = value;
    }

    public static string GetAlchemyLootString(AlchemyLoot lootString)
    {
        return System.Text.RegularExpressions.Regex.Replace(lootString.ToString(), "(?<!^)([A-Z])", " $1");
    }

    public static string GetCraftingPotentialString()
    {
        string inv = "";
        foreach (KeyValuePair<CraftingPotential, int> kvp in AvailableCraftingPotential)
        {
            if (kvp.Value != 0)
                inv += $"{GeneralFunctions.GetCleanEnumString(kvp.Key)}: {kvp.Value}\n";
        }
        return inv;
    }
    public static string GetElementalKnowlegdeString()
    {
        string knowledge = "";
        foreach (KeyValuePair<Element, int> kvp in knowledgeDict)
        {
            if (kvp.Value != 0)
                knowledge += $"{kvp.Key}: {kvp.Value}\n";
        }
        return knowledge;
    }
    public static string GetKnownToolsString()
    {
        string tools = "";
        foreach (AlchemyTool tool in KnownTools)
        {
            tools += $"{tool}";
        }
        return tools;
    }
}
