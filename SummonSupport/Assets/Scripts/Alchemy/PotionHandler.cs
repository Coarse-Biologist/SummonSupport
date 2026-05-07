
using UnityEngine;
using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine.InputSystem.Interactions;

public class PotionHandler : MonoBehaviour
{
    public static PotionHandler Instance;
    [field: SerializeField] public GameObject AcidPotion { private set; get; }

    [field: SerializeField] public GameObject BacterialPotion { private set; get; }

    [field: SerializeField] public GameObject ViralPotion { private set; get; }

    [field: SerializeField] public GameObject FungalPotion { private set; get; }

    [field: SerializeField] public GameObject PlantPotion { private set; get; }

    [field: SerializeField] public GameObject HeatPotion { private set; get; }

    [field: SerializeField] public GameObject ColdPotion { private set; get; }

    [field: SerializeField] public GameObject ElectricityPotion { private set; get; }

    [field: SerializeField] public GameObject RadiationPotion { private set; get; }

    [field: SerializeField] public GameObject PsychicPotion { private set; get; }

    [field: SerializeField] public GameObject LightPotion { private set; get; }

    [field: SerializeField] public GameObject EarthPotion { private set; get; }

    [field: SerializeField] public GameObject AirPotion { private set; get; }

    [field: SerializeField] public GameObject WaterPotion { private set; get; }

    public static Dictionary<Element, GameObject> ElementToPotion { private set; get; }
    public static Dictionary<int, GameObject> AbilitySlotToPotion { private set; get; }
    private GameObject PotionInHand;

    void Awake()
    {
        ElementToPotion = new Dictionary<Element, GameObject>()
        {
            { Element.Bacteria, BacterialPotion },
            { Element.Virus, ViralPotion },
            { Element.Fungi, FungalPotion },
            { Element.Plant, PlantPotion },
            { Element.Heat, HeatPotion },
            { Element.Cold, ColdPotion },
            { Element.Electricity, ElectricityPotion },
            { Element.Radiation, RadiationPotion },
            { Element.Psychic, PsychicPotion },
            { Element.Light, LightPotion },
            { Element.Earth, EarthPotion },
            { Element.Air, AirPotion },
            { Element.Water, WaterPotion }
        };
        AbilitySlotToPotion = new Dictionary<int, GameObject>();

    }
    void OnEnable()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;
        EventDeclarer.SlotChanged.AddListener(SpawnElementalPotionOnBelt);
    }

    // #TODO somewhere have a Dictionary with positions for all potions

    //#TODO make a function that spawns potions on toolbelt depending on equipped abilities.

    // #TODO make a function that moves the potions from the tool belt to the hand for the ability / crafting reloading animation

    //#TODO make a function that moves potion back to belt after reload animation. 
    // This can be called whenever an ability is used which should replace the one currently in hand


    public static void SpawnElementalPotionOnBelt(int abilityslot, Ability ability)
    {

        if (AbilitySlotToPotion.TryGetValue(abilityslot, out GameObject PreviouslyEquipped)) Destroy(PreviouslyEquipped);
        Element element = Element.None;
        if (ability.ElementTypes.Count > 0) element = ability.ElementTypes[0]; // for now just take the first element, eventually we can have a way to show multiple elements on the potion
        if (ElementToPotion.TryGetValue(element, out GameObject obj))
        {
            Transform spawnLoc = PlayerStats.Instance.AbilityPotionTransformList[abilityslot];
            GameObject instance = Instantiate(obj, spawnLoc.position, Quaternion.identity, spawnLoc);
            AbilitySlotToPotion[abilityslot] = instance;

        }
    }

    public static void MovePotionToHandorBelt(int abilitySlot, bool toHand)
    {
        Transform targetTransform = toHand ? PlayerStats.Instance.HandTransform : PlayerStats.Instance.AbilityPotionTransformList[abilitySlot];

        if (AbilitySlotToPotion.TryGetValue(abilitySlot, out GameObject abilityPotion))
        {
            abilityPotion.transform.position = targetTransform.position;
            abilityPotion.transform.SetParent(targetTransform);
        }
    }

}