using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;
using SummonSupportEvents;
using UnityEditor.Build.Pipeline.Tasks;
using System.Linq;
using NUnit.Framework.Constraints;
public abstract class LivingBeing : MonoBehaviour
{
    #region Declarations

    [field: Header("Birth certificate")]
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public CharacterTag CharacterTag { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public List<string> BattleCries { get; private set; }

    #region Resources
    [field: Header("Attributes - Resources")]
    [field: SerializeField] public float MaxHP { get; private set; } = 100;
    [field: SerializeField] public float Overshield { get; private set; }
    [field: SerializeField] public float CurrentHP { get; private set; } = 100;
    [field: SerializeField] public float MaxPower { get; private set; } = 100;
    [field: SerializeField] public float PowerSurge { get; private set; }
    [field: SerializeField] public float CurrentPower { get; private set; } = 100;
    #endregion
    #region Health regen variables

    [field: Header("Attributes - Regenerations")]
    private WaitForSeconds regenTickRate;
    [field: SerializeField] public float TickRateRegenerationInSeconds { get; private set; } = .2f;
    [field: SerializeField] public float HealthRegeneration { get; private set; } = 0;
    [field: SerializeField] public float PowerRegeneration { get; private set; } = 1;
    [field: SerializeField] public float TotalHealthRegeneration { get; private set; } = 0; // i think this is / should be a private variable since it is only used for local calculations? or at least i think it doesnt need to be serialized since it would never be used (it is instantluy set to the value of HealthRegeneration)
    [field: SerializeField] public float TotalPowerRegeneration { get; private set; } = 0;
    [field: SerializeField] public int HealthRegenArrows { get; private set; } = 0;
    [field: SerializeField] public int PowerRegenArrows { get; private set; } = 0;
    [field: SerializeField] public float RegenCalcOffset { get; private set; } = .8f;
    [field: SerializeField] public int MaxRegenArrows { get; private set; } = 6;
    #endregion

    //TODO:

    #region Affinity Stats

    [field: Header("Affinity Stats")]
    [field: SerializeField] protected float Cold { get; private set; } = 0;
    [field: SerializeField] protected float Water { get; private set; } = 0;
    [field: SerializeField] protected float Earth { get; private set; } = 0;
    [field: SerializeField] protected float Heat { get; private set; } = 0;
    [field: SerializeField] protected float Air { get; private set; } = 0;
    [field: SerializeField] protected float Electricity { get; private set; } = 0;
    [field: SerializeField] protected float Poison { get; private set; } = 0;
    [field: SerializeField] protected float Acid { get; private set; } = 0;
    [field: SerializeField] protected float Bacteria { get; private set; } = 0;
    [field: SerializeField] protected float Fungi { get; private set; } = 0;
    [field: SerializeField] protected float Plant { get; private set; } = 0;
    [field: SerializeField] protected float Virus { get; private set; } = 0;
    [field: SerializeField] protected float Radiation { get; private set; } = 0;
    [field: SerializeField] protected float Light { get; private set; } = 0;
    [field: SerializeField] protected float Psychic { get; private set; } = 0;
    #endregion

    #region Armor Stats
    [field: Header("Armor Stats")]

    [field: SerializeField] protected float Piercing { get; private set; } = 0;
    [field: SerializeField] protected float Bludgeoning { get; private set; } = 0;
    [field: SerializeField] protected float Slashing { get; private set; } = 0;
    public Dictionary<PhysicalType, (Func<float> Get, Action<float> Set)> PhysicalDict { private set; get; } = new();

    #endregion

    #region other data

    [Header("Other")]
    [field: SerializeField] public List<Ability> AffectedByAbilities { get; private set; } = new();
    public List<StatusEffectType> SufferedStatusEffects { get; private set; } = new();

    [field: SerializeField] public float XP_OnDeath { get; private set; } = 5f;
    public bool Dead { get; private set; } = false;

    public Dictionary<Element, (Func<float> Get, Action<float> Set)> Affinities { private set; get; } = new();
    public Dictionary<AttributeType, (Func<float> Get, Action<float> Set)> ResourceAttributesDict { private set; get; } = new();


    private I_ResourceBar resourceBarInterface;
    private AbilityHandler abilityHandler;

    #endregion

    #endregion

    protected virtual void Awake()
    {
        InitializeAttributeDict();
        InitializeAffinityDict();
        InitializePhysicalDict();
        resourceBarInterface = GetComponent<I_ResourceBar>();
        regenTickRate = new WaitForSeconds(TickRateRegenerationInSeconds);

    }

    protected virtual void Start()
    {
        InitializeRegenerationValues();
        StartCoroutine(RegenerateRoutine());
        abilityHandler = GetComponent<AbilityHandler>();
    }

    private void InitializeRegenerationValues()
    {
        TotalHealthRegeneration = HealthRegeneration;
        TotalPowerRegeneration = PowerRegeneration;
    }




    #region Resource Control

    public void RestoreResources()
    {
        CurrentHP = MaxHP;
        CurrentPower = MaxPower;
    }

    #endregion

    #region Affinity handling
    public void ChangeAffinity(Element element, float amount)
    {
        float newAffinity = Mathf.Min(amount + Affinities[element].Get(), 200);

        if (Affinities.TryGetValue(element, out (Func<float> Get, Action<float> Set) func))
        {
            Affinities[element].Set(newAffinity);
        }
        Debug.Log($"Change affinity: current Affinity = {newAffinity}");

    }
    public void SetAffinity(Element element, float amount)
    {
        float newAffinity = Mathf.Min(amount, 200);
        newAffinity = Mathf.Max(newAffinity, 0);


        if (Affinities.TryGetValue(element, out (Func<float> Get, Action<float> Set) func))
        {
            Affinities[element].Set(newAffinity);
        }
        Debug.Log($"SetAffinity: Affinity = {newAffinity}");
    }
    public int GetAffinity(Element element)
    {
        if (element == Element.None) return 0;
        return (int)Affinities[element].Get();
    }

    public Element GetHighestAffinity()
    {
        Element element = Affinities.OrderByDescending(a => a.Value.Get()).First().Key;
        if (Affinities[element].Get() < 20) return Element.None;
        else return element;
    }

    #endregion

    #region Physical resistance handling

    public void SetPhysicalResist(PhysicalType physicalType, float value)
    {
        float newResistance = Mathf.Min(value, 200);

        if (PhysicalDict.TryGetValue(physicalType, out (Func<float> Get, Action<float> Set) func))
        {
            PhysicalDict[physicalType].Set(newResistance);
        }
    }

    public void ChangePhysicalResistance(PhysicalType physicalType, float amount)
    {
        float newResistance = Mathf.Min(amount + PhysicalDict[physicalType].Get(), 200);

        if (PhysicalDict.TryGetValue(physicalType, out (Func<float> Get, Action<float> Set) func))
        {
            PhysicalDict[physicalType].Set(newResistance);
        }
    }
    #endregion

    #region Attribute Handling

    protected void SetDead(bool isDead)
    {
        Dead = isDead;
    }

    public float GetAttribute(AttributeType attribute)
    {
        if (ResourceAttributesDict != null && ResourceAttributesDict.ContainsKey(attribute))
            return ResourceAttributesDict[attribute].Get();
        else
            throw new Exception("Attribute not found");
    }

    public void SetAttribute(AttributeType attributeType, float value)
    {
        value = (float)Math.Floor(value);
        value = Math.Max(0, value);

        if (ResourceAttributesDict != null && ResourceAttributesDict.ContainsKey(attributeType))
            ResourceAttributesDict[attributeType].Set(value);
        HandleEventInvokes(attributeType, value);
        if (GetAttribute(AttributeType.CurrentHitpoints) <= 0)
            Die();
        else if (attributeType == AttributeType.CurrentPower && value <= 0)
            abilityHandler.HandleNoMana();
    }

    public float ChangeAttribute(AttributeType attributeType, float value)
    {
        //if (GetAttribute(AttributeType.CurrentHitpoints) <= 0) // do nothing if dead
        //    return 0f;

        if (ResourceAttributesDict == null || !ResourceAttributesDict.ContainsKey(attributeType))
            throw new Exception("Attribute not found or invalid setter");

        SetAttribute(attributeType, GetAttribute(attributeType) + value);

        if (GetAttribute(AttributeType.CurrentHitpoints) <= 0)
            Die();
        return GetAttribute(attributeType) + value;
    }

    public void HandleEventInvokes(AttributeType attributeType, float newValue)
    {
        switch (attributeType)
        {
            case AttributeType.MaxHitpoints:
                if (CharacterTag != CharacterTag.Enemy)
                    EventDeclarer.maxAttributeChanged?.Invoke(this, attributeType);
                resourceBarInterface?.SetHealthBarMaxValue(GetAttribute(attributeType));
                break;

            case AttributeType.CurrentHitpoints:
                if (CharacterTag != CharacterTag.Enemy)
                    EventDeclarer.attributeChanged?.Invoke(this, attributeType);
                resourceBarInterface?.SetHealthBarValue(GetAttribute(attributeType));
                break;

            case AttributeType.MaxPower:
                if (CharacterTag != CharacterTag.Enemy)
                    EventDeclarer.maxAttributeChanged?.Invoke(this, attributeType);
                resourceBarInterface?.SetPowerBarMaxValue(GetAttribute(attributeType));
                break;

            case AttributeType.CurrentPower:
                if (CharacterTag != CharacterTag.Enemy)
                    EventDeclarer.attributeChanged?.Invoke(this, attributeType);
                resourceBarInterface?.SetPowerBarValue(GetAttribute(attributeType));
                break;
            default:
                break;
                //case AttributeType.MovementSpeed:
                //case AttributeType.DashBoost:
                //case AttributeType.DashCooldown:
                //case AttributeType.DashDuration:
                //EventDeclarer.SpeedAttributeChanged.Invoke(attributeType, newValue);
                //break;
        }
    }


    #endregion

    #region Changing list of abilities affecting one

    public void AlterAbilityList(Ability ability, bool Add) // modifies the list of abilities by which one is affected
    {
        bool contains = AffectedByAbilities.Contains(ability);
        //  Debug.Log($"Alter ability func: add = {Add}. Contains = {contains}");
        if (Add && !contains) AffectedByAbilities.Add(ability);
        if (!Add && contains) AffectedByAbilities.Remove(ability);
    }
    public bool IsAffectedByAbility(Ability ability)
    {
        return AffectedByAbilities.Contains(ability);
    }
    public void AlterStatusEffectList(StatusEffectType status, bool Add) // modifies the list of abilities by which one is affected
    {

        bool contains = SufferedStatusEffects.Contains(status);
        if (Add && !contains) SufferedStatusEffects.Add(status);
        if (!Add && contains) SufferedStatusEffects.Remove(status);
    }
    public bool HasStatusEffect(StatusEffectType status)
    {
        return SufferedStatusEffects.Contains(status);
    }



    #endregion

    #region Handle Regeneration
    private IEnumerator RegenerateRoutine()
    {
        while (true)
        {
            float newHP = Mathf.Min(CurrentHP + TotalHealthRegeneration, MaxHP);
            if (newHP != CurrentHP)
                SetAttribute(AttributeType.CurrentHitpoints, newHP);
            float newPower = Mathf.Min(CurrentPower + TotalPowerRegeneration, MaxPower);
            if (newPower != CurrentPower)
                SetAttribute(AttributeType.CurrentPower, newPower);
            yield return regenTickRate;
        }
    }

    public void ChangeRegeneration(AttributeType attributeType, float value)
    {
        if (attributeType == AttributeType.CurrentHitpoints)
        {
            TotalHealthRegeneration += value;
            HealthRegenArrows = GetRegenerationIndicatorAmount(MaxHP, TotalHealthRegeneration);
        }
        else if (attributeType == AttributeType.CurrentPower)
        {
            TotalPowerRegeneration += value;
            PowerRegenArrows = GetRegenerationIndicatorAmount(MaxPower, TotalPowerRegeneration);
        }
    }

    int GetRegenerationIndicatorAmount(float maxValue, float regeneration)
    {
        float regenerationIndicatorStep = maxValue * (1 - RegenCalcOffset) / MaxRegenArrows;
        float arrows = regeneration / regenerationIndicatorStep;
        float clampedArrows;
        if (arrows > 0)
            clampedArrows = Mathf.Clamp(arrows, 1f, MaxRegenArrows); // 0.1 arrows should be 1, 100 arrows schould be value of MaxRegenArrows
        else if (arrows < 0)
            clampedArrows = Mathf.Clamp(arrows, -MaxRegenArrows, -1f); // -0.1 arrows should be -1, -100 arrows schould be value of -MaxRegenArrows
        else
            clampedArrows = 0;
        int roundedArrows = Mathf.RoundToInt(clampedArrows);
        return roundedArrows;
    }
    #endregion

    #region Niche attribute changes
    public void SetCharacterTag(CharacterTag tag)
    {
        CharacterTag = tag;
    }
    public void SetName(string newName)
    {
        Name = newName;
    }

    public void LearnBattleCry(string newBattleCry)
    {
        if (!BattleCries.Contains(newBattleCry)) BattleCries.Add(newBattleCry);
    }

    #endregion

    #region Dictionary Setup

    void InitializeAttributeDict()
    {
        ResourceAttributesDict = new Dictionary<AttributeType, (Func<float> Get, Action<float> Set)>
            {
                { AttributeType.MaxHitpoints,           (() => MaxHP,               v => MaxHP = v) },
                { AttributeType.Overshield,             (() => Overshield,      v => Overshield = v) },
                { AttributeType.CurrentHitpoints,       (() => CurrentHP,           v => CurrentHP = v) },
                { AttributeType.MaxPower,               (() => MaxPower,            v => MaxPower = v) },
                { AttributeType.PowerSurge,             (() => PowerSurge,   v => PowerSurge = v) },
                { AttributeType.CurrentPower,           (() => CurrentPower,        v => CurrentPower = v) },
                //{ AttributeType.MovementSpeed,          (() => Speed,        v => Speed = v) },
            };
    }

    void InitializeAffinityDict()
    {
        Affinities = new Dictionary<Element, (Func<float> Get, Action<float> Set)>
            {
                { Element.Cold,            (() => Cold,            v => Cold = v) },
                { Element.Water,           (() => Water,           v => Water = v) },
                { Element.Earth,           (() => Earth,           v => Earth = v) },
                { Element.Heat,            (() => Heat,            v => Heat = v) },
                { Element.Air,             (() => Air,             v => Air = v) },
                { Element.Electricity,     (() => Electricity,     v => Electricity = v) },
                { Element.Poison,          (() => Poison,          v => Poison = v) },
                { Element.Acid,            (() => Acid,            v => Acid = v) },
                { Element.Bacteria,        (() => Bacteria,        v => Bacteria = v) },
                { Element.Fungi,           (() => Fungi,           v => Fungi = v) },
                { Element.Plant,           (() => Plant,           v => Plant = v) },
                { Element.Virus,           (() => Virus,           v => Virus = v) },
                { Element.Radiation,       (() => Radiation,       v => Radiation = v) },
                { Element.Light,           (() => Light,           v => Light = v) },
                { Element.Psychic,         (() => Psychic,         v => Psychic = v) }
            };
    }
    void InitializePhysicalDict()
    {
        PhysicalDict = new Dictionary<PhysicalType, (Func<float> Get, Action<float> Set)>
            {
                { PhysicalType.Piercing,       (() => Piercing,       v => Piercing = v) },
                { PhysicalType.Bludgeoning,    (() => Bludgeoning,    v => Bludgeoning = v) },
                { PhysicalType.Slashing,       (() => Slashing,        v => Slashing = v) }
            };
    }

    #endregion

    public abstract void Die();


    protected void ViciousDeathExplosion()
    {
        Debug.Log("Softy died and caused chain reaction");
    }
}
