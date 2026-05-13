
using UnityEngine;
using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine.InputSystem.Interactions;
using System;

public class PotionHandler : MonoBehaviour
{
    #region potions
    [Header("Potions")]
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
    #endregion
    #region Foams

    [Header("Foams")]
    [field: SerializeField] public GameObject AcidFoam { private set; get; }

    [field: SerializeField] public GameObject BacterialFoam { private set; get; }

    [field: SerializeField] public GameObject ViralFoam { private set; get; }

    [field: SerializeField] public GameObject FungalFoam { private set; get; }

    [field: SerializeField] public GameObject PlantFoam { private set; get; }

    [field: SerializeField] public GameObject HeatFoam { private set; get; }

    [field: SerializeField] public GameObject ColdFoam { private set; get; }

    [field: SerializeField] public GameObject ElectricityFoam { private set; get; }

    [field: SerializeField] public GameObject PoisonFoam { private set; get; }


    [field: SerializeField] public GameObject RadiationFoam { private set; get; }

    [field: SerializeField] public GameObject PsychicFoam { private set; get; }

    [field: SerializeField] public GameObject LightFoam { private set; get; }

    [field: SerializeField] public GameObject EarthFoam { private set; get; }

    [field: SerializeField] public GameObject AirFoam { private set; get; }

    [field: SerializeField] public GameObject WaterFoam { private set; get; }
    #endregion
    #region Dictionaories for lookup
    public static Dictionary<Element, GameObject> ElementToPotion { private set; get; }

    public static Dictionary<int, GameObject> AbilitySlotToPotion { private set; get; } = new();
    #endregion
    private static Tuple<int, GameObject> PotionInHand = new(-1, null); // item 1 is the ability slot the potion belongs to, item 2 is the potion gameobject itself. if item 1 is -1, then there is no potion in hand.


    void Awake()
    {
        ElementToPotion = ElementDict.GetElementDict(
            new GameObject[] { ColdPotion, WaterPotion, EarthPotion, HeatPotion, AirPotion, ElectricityPotion, PoisonFoam, AcidPotion, BacterialPotion, FungalPotion, PlantPotion, ViralPotion, RadiationPotion, LightPotion, PsychicPotion }
            );
    }
    void OnEnable()
    {
        EventDeclarer.AbilityUsed?.AddListener(MovePotionToHand);
        EventDeclarer.SlotChanged.AddListener(SpawnElementalPotionOnBelt);
    }
    void OnDisable()
    {
        EventDeclarer.AbilityUsed?.AddListener(MovePotionToHand);

        EventDeclarer.SlotChanged.RemoveListener(SpawnElementalPotionOnBelt);
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
            AbilitySlotToPotion.TryAdd(abilityslot, instance);
        }
        else
        {
            Transform spawnLoc = PlayerStats.Instance.AbilityPotionTransformList[abilityslot];
            GameObject instance = Instantiate(ElementToPotion[Element.Plant], spawnLoc.position, Quaternion.identity, spawnLoc);
            AbilitySlotToPotion.TryAdd(abilityslot, instance);

            //#TODO find more thorough and thoughtful solution// currently spawn plant potion if  the ability has no element. what should the true fix be later?
        }
    }

    public static void MovePotionToHand(int abilitySlot)
    {

        if (AbilitySlotToPotion.TryGetValue(abilitySlot, out GameObject abilityPotion))
        {
            abilityPotion.transform.position = PlayerStats.Instance.HandTransform.position;
            abilityPotion.transform.SetParent(PlayerStats.Instance.HandTransform);

            if (abilityPotion.GetComponentInChildren<ParticleSystem>() is ParticleSystem ps) ps.Play(); // stop particle system so it doesn't look weird when the potion moves to the hand.

            //else if (abilityPotion.GetComponentInChildren<ParticleSystem>() is ParticleSystem ps) ps.Stop();
            PotionInHand = new Tuple<int, GameObject>(abilitySlot, abilityPotion);
        }
    }

    public static void ReturnPotionToBelt()
    {
        if (PotionInHand.Item1 == -1 || PotionInHand.Item2 == null)
        {
            throw new Exception("ReturnPotionToBelt was called but there is no potion in hand.");
        }
        PotionInHand.Item2.transform.position = PlayerStats.Instance.AbilityPotionTransformList[PotionInHand.Item1].position;
        PotionInHand.Item2.transform.SetParent(PlayerStats.Instance.AbilityPotionTransformList[PotionInHand.Item1]);
    }

}