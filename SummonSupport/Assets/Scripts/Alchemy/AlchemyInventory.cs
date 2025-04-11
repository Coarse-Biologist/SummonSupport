using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Alchemy
{
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

        public static Dictionary<Elements, int> knowledgeDict = new Dictionary<Elements, int>
            {
                { Elements.Cold, 0 },
                { Elements.Water, 0 },
                { Elements.Earth, 0 },
                { Elements.Heat, 0 },
                { Elements.Air, 0 },
                { Elements.Electricity, 0 },
                { Elements.Poison, 0 },
                { Elements.Acid, 0 },
                { Elements.Bacteria, 0 },
                { Elements.Fungi, 0 },
                { Elements.Plant, 0 },
                { Elements.Virus, 0 },
                { Elements.Radiation, 0 },
                { Elements.Light, 0 },
                { Elements.Psychic, 0 }
            };

        public static List<AlchemyTools> KnownTools = new List<AlchemyTools>();

        #endregion

        #region Set Dict values
        public static void IncemementElementalKnowledge(Elements element, int amount)
        {
            knowledgeDict[element] += amount;
        }
        public static void AlterIngredientNum(AlchemyLoot ingredient, int amount)
        {
            Logging.Info($"You have gained {amount} {ingredient}");
            ingredients[ingredient] += amount;

        }
        public static void ExpendIngredients(Dictionary<AlchemyLoot, int> usedIngredients)
        {
            foreach (KeyValuePair<AlchemyLoot, int> kvp in usedIngredients)
            {
                if (ingredients[kvp.Key] > 0) AlterIngredientNum(kvp.Key, -kvp.Value);
            }
        }
        public static void GainTool(AlchemyTools tool)
        {
            if (!KnownTools.Contains(tool))
            {
                Logging.Info($"{tool} gained!");
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

    public enum AlchemyLoot
    {
        WretchedOrgans,
        FunctionalOrgans,
        HulkingOrgans,
        BrokenCores,
        WorkingCore,
        PowerfulCore,
        HulkingCore,
        FaintEther,
        PureEther,
        IntenseEther,
    }

    public enum AlchemyTools
    {
        Beaker,
        AccurateWeights,
        Thermometer,
        Barometer,
        Pipette,
        MicroPipette,
        VolumetricFlask,
        MagnifyingGlass,
        ArcSpring,
        Filter,
        Centrifuge,
        LightMicroscope,
        CompoundMicroscope,
        DarkFieldMicroscope,
        PhaseShiftMicroscope,
        TransmissionElectronMicroscrope,
        ScannningElectronMicroscrope,
        NuclearMagneticResonator,
        MassSpectrometer,
        Chromatograph,
    }

    public enum Elements
    {
        None,
        Cold,
        Water,
        Earth,
        Heat,
        Air,
        Electricity,
        Poison,
        Acid,
        Bacteria,
        Fungi,
        Plant,
        Virus,
        Radiation,
        Light,
        Psychic
    }

}