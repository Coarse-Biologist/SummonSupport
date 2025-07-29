using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;
using SummonSupportEvents;
using UnityEditor.Build.Pipeline.Tasks;
public abstract class LivingBeing : MonoBehaviour
{
    #region Declarations

    [Header("Birth certificate")]
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public CharacterTag CharacterTag { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public List<string> BattleCries { get; private set; }

    #region Resources
    [Header("Attributes - Resources")]
    [field: SerializeField] public float MaxHP { get; private set; } = 100;
    [field: SerializeField] public float Overshield { get; private set; }
    [field: SerializeField] public float CurrentHP { get; private set; } = 100;
    [field: SerializeField] public float MaxPower { get; private set; } = 100;
    [field: SerializeField] public float PowerSurge { get; private set; }
    [field: SerializeField] public float CurrentPower { get; private set; } = 100;
    #endregion
    #region Health regen variables

    [Header("Attributes - Regenerations")]
    private WaitForSeconds regenTickRate;
    [field: SerializeField] public float TickRateRegenerationInSeconds { get; private set; } = .2f;
    [field: SerializeField] public float HealthRegeneration { get; private set; } = 0;
    [field: SerializeField] public float PowerRegeneration { get; private set; } = 1;
    [field: SerializeField] public float TotalHealthRegeneration { get; private set; } = 0;
    [field: SerializeField] public float TotalPowerRegeneration { get; private set; } = 0;
    [field: SerializeField] public int HealthRegenArrows { get; private set; } = 0;
    [field: SerializeField] public int PowerRegenArrows { get; private set; } = 0;
    [field: SerializeField] public float RegenCalcOffset { get; private set; } = .8f;
    [field: SerializeField] public int MaxRegenArrows { get; private set; } = 6;
    #endregion

    //TODO:

    #region Affinity Stats

    [Header("Affinity Stats")]
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
    [field: SerializeField] protected float Piercing { get; private set; } = 0;
    [field: SerializeField] protected float Bludgeoning { get; private set; } = 0;
    [field: SerializeField] protected float Slashing { get; private set; } = 0;
    public Dictionary<PhysicalType, (Func<float> Get, Action<float> Set)> PhysicalDict { private set; get; } = new();


    #endregion

    #region other data

    [Header("Other")]
    [field: SerializeField] public List<Ability> AffectedByAbilities { get; private set; } = new();
    [field: SerializeField] public float XP_OnDeath { get; private set; } = 5f;

    public Dictionary<Element, (Func<float> Get, Action<float> Set)> Affinities { private set; get; } = new();
    public Dictionary<AttributeType, (Func<float> Get, Action<float> Set)> AttributesDict { private set; get; } = new();

    //[field: SerializeField] public List<Crew_Ability_SO> Abilties { get; private set; } = new();
    [field: SerializeField] public float Speed { get; private set; } = 3f;
    [field: SerializeField] public float Mass { get; private set; } = 1f;

    private I_ResourceBar resourceBarInterface;

    #endregion

    #endregion

    protected virtual void Awake()
    {
        GetComponent<Rigidbody2D>().mass = Mass;
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
    public void GainAffinity(Element element, float amount)
    {
        float newAffinity = Mathf.Min(amount + Affinities[element].Get(), 200);

        if (Affinities.TryGetValue(element, out (Func<float> Get, Action<float> Set) func))
        {
            Affinities[element].Set(newAffinity);
        }
    }
    #endregion
    #region Attribute Handling

    public float GetAttribute(AttributeType attribute)
    {
        if (AttributesDict != null && AttributesDict.ContainsKey(attribute))
            return AttributesDict[attribute].Get();
        else
            throw new Exception("Attribute not found");
    }

    public void SetAttribute(AttributeType attributeType, float value)
    {
        if (AttributesDict != null && AttributesDict.ContainsKey(attributeType))
            AttributesDict[attributeType].Set(value);
        HandleEventInvokes(attributeType, value);
        if (attributeType == AttributeType.CurrentHitpoints && value <= 0)
            Die();
    }

    public float ChangeAttribute(AttributeType attributeType, float value)
    {
        if (AttributesDict == null || !AttributesDict.ContainsKey(attributeType))
            throw new Exception("Attribute not found or invalid setter");

        //float newValue = CalculateNewValueByType(currentValue, value, valueType, attributeType);
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

            case AttributeType.MovementSpeed:
            case AttributeType.DashBoost:
            case AttributeType.DashCooldown:
            case AttributeType.DashDuration:
                EventDeclarer.SpeedAttributeChanged.Invoke(attributeType, newValue);
                break;
        }
    }


    #endregion

    #region Changing list of abilities affecting one

    public void AlterAbilityList(Ability ability, bool Add) // modifies the list of abilities by which one is affected
    {
        bool contains = AffectedByAbilities.Contains(ability);
        if (Add && !contains) AffectedByAbilities.Add(ability);
        if (!Add && contains) AffectedByAbilities.Remove(ability);
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
    public void SetName(string newName)
    {
        Name = newName;
    }
    public void Gainmass(float massGain)
    {
        Mass += massGain;
        GetComponent<Rigidbody2D>().mass += massGain;
    }
    public void ChangeSpeed(float amount)
    {
        Speed += amount;
    }
    public void LearnBattleCry(string newBattleCry)
    {
        if (!BattleCries.Contains(newBattleCry)) BattleCries.Add(newBattleCry);
    }

    #endregion

    #region Dictionary Setup

    void InitializeAttributeDict()
    {
        AttributesDict = new Dictionary<AttributeType, (Func<float> Get, Action<float> Set)>
            {
                { AttributeType.MaxHitpoints,           (() => MaxHP,               v => MaxHP = v) },
                { AttributeType.Overshield,             (() => Overshield,      v => Overshield = v) },
                { AttributeType.CurrentHitpoints,       (() => CurrentHP,           v => CurrentHP = v) },
                { AttributeType.MaxPower,               (() => MaxPower,            v => MaxPower = v) },
                { AttributeType.PowerSurge,             (() => PowerSurge,   v => PowerSurge = v) },
                { AttributeType.CurrentPower,           (() => CurrentPower,        v => CurrentPower = v) },
                { AttributeType.MovementSpeed,          (() => Speed,        v => Speed = v) },
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
    protected void Die()
    {
        Logging.Info($"{gameObject.name} died");
        if (gameObject.GetComponent<MinionStats>() != null) EventDeclarer.minionDied?.Invoke(this);
        Destroy(gameObject);
    }
}
