using System.Collections.Generic;
using UnityEngine;
namespace Alchemy
{
    public static class AlchemyInventory
    {
        #region Class variables
        public static Dictionary<AlchemyLoot, int> ingredients { get; private set; } = new Dictionary<AlchemyLoot, int>
        {
            { AlchemyLoot.BrokenCores, 2 },
            { AlchemyLoot.WorkingCore, 0 },
            { AlchemyLoot.PowerfulCore, 0 },
            { AlchemyLoot.HulkingCore, 0 },
            { AlchemyLoot.FaintEther, 0 },
            { AlchemyLoot.PureEther, 0 },
            { AlchemyLoot.IntenseEther, 0 }
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
        #endregion

        #region Set Dict values
        public static void IncemementElementalKnowledge(Elements element, int amount)
        {
            knowledgeDict[element] += amount;
        }
        public static void AlterIngredientNum(AlchemyLoot ingredient, int amount)
        {
            ingredients[ingredient] += amount;
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
        NuclearMagneticResonanator,
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