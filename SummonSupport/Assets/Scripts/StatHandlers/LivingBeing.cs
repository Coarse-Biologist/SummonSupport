using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using SummonSupportEvents;
public abstract class LivingBeing : MonoBehaviour
{
    #region Declarations

    [Header("Birth certificate")]
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public CharacterTag CharacterTag { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public List<string> BattleCries { get; private set; }

    [Header("Attributes - Resources")]
    [field: SerializeField] public float MaxHP { get; private set; } = 100;
    [field: SerializeField] public float Overshield { get; private set; }
    [field: SerializeField] public float CurrentHP { get; private set; } = 100;
    [field: SerializeField] public float MaxPower { get; private set; } = 100;
    [field: SerializeField] public float PowerSurge { get; private set; }
    [field: SerializeField] public float CurrentPower { get; private set; } = 100;

    [Header("Attributes - Regenerations")]

    [field: SerializeField] public float HealthRegeneration { get; private set; } = 1;
    [field: SerializeField] public float PowerRegeneration { get; private set; } = 1;

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
    #endregion Afinity Stats

    #region other data

    [Header("Other")]
    [field: SerializeField] public float XP_OnDeath { get; private set; } = 3;
    public Dictionary<string, StatusEffectInstance> activeStatusEffects = new();
    public Dictionary<Element, (Func<float> Get, Action<float> Set)> Affinities { private set; get; } = new();
    public Dictionary<AttributeType, (Func<float> Get, Action<float> Set)> AttributesDict { private set; get; } = new();

    [field: SerializeField] public List<Ability> Abilties { get; private set; } = new();
    [field: SerializeField] public float Speed { get; private set; } = 0.4f;
    [field: SerializeField] public float Mass { get; private set; } = 1f;

    private I_ResourceBar resourceBarInterface;

    #endregion

    #endregion

    protected virtual void Awake()
    {
        GetComponent<Rigidbody2D>().mass = Mass;
        InitializeAttributeDict();
        InitializeAffinityDict();
        resourceBarInterface = GetComponent<I_ResourceBar>();
        InvokeRepeating("RegenerateMana", 0f, 1f);
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
        if (Affinities.TryGetValue(element, out (Func<float> Get, Action<float> Set) func))
        {
            Affinities[element].Set(amount + Affinities[element].Get());
        }
    }
    #endregion

    #region Status effects handling
    private System.Collections.IEnumerator ExecuteEverySecond(Action action, float duration)
    {
        float timePassed = 0f;
        while (timePassed < duration)
        {
            action?.Invoke();
            yield return new WaitForSeconds(1f);
            timePassed += 1f;
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

    }

    public float ChangeAttribute(AttributeType attributeType, float value, ValueType valueType = ValueType.Flat)
    {
        if (AttributesDict == null || !AttributesDict.ContainsKey(attributeType))
            throw new Exception("Attribute not found or invalid setter");

        float currentValue = GetAttribute(attributeType);
        float newValue = CalculateNewValueByType(currentValue, value, valueType, attributeType);
        SetAttribute(attributeType, newValue);

        if (GetAttribute(AttributeType.CurrentHitpoints) <= 0)
            Die();
        return newValue;
    }

    public void HandleEventInvokes(AttributeType attributeType, float newValue)
    {
        switch (attributeType)
        {
            case AttributeType.MaxHitpoints:
            case AttributeType.CurrentHitpoints:
                if (CharacterTag != CharacterTag.Enemy)
                {
                    EventDeclarer.maxAttributeChanged?.Invoke(this, attributeType);
                    EventDeclarer.attributeChanged?.Invoke(this, attributeType);
                }

                resourceBarInterface?.SetHealthBarMaxValue(GetAttribute(attributeType));
                resourceBarInterface?.SetHealthBarValue(GetAttribute(attributeType));

                break;
            case AttributeType.MaxPower:
            case AttributeType.CurrentPower:
                if (CharacterTag != CharacterTag.Enemy)
                {
                    EventDeclarer.maxAttributeChanged?.Invoke(this, attributeType);
                    EventDeclarer.attributeChanged?.Invoke(this, attributeType);
                }

                resourceBarInterface?.SetPowerBarValue(GetAttribute(attributeType));
                resourceBarInterface?.SetPowerBarMaxValue(GetAttribute(attributeType));


                break;

            case AttributeType.MovementSpeed:
            case AttributeType.DashBoost:
            case AttributeType.DashCooldown:
            case AttributeType.DashDuration:
                EventDeclarer.SpeedAttributeChanged.Invoke(attributeType, newValue);
                break;
        }
    }

    float CalculateNewValueByType(float currentValue, float value, ValueType valueType, AttributeType attributeType)
    {
        float newValue = valueType == ValueType.Percentage
        ? currentValue * (1 + value / 100f)
        : currentValue + value;

        return HandleAttributeCap(attributeType, newValue, currentValue, value);
    }

    float HandleAttributeCap(AttributeType attributeType, float newValue, float currentValue, float delta)
    {
        switch (attributeType)
        {
            case AttributeType.CurrentHitpoints:
                newValue = ApplyCap(AttributeType.MaxHitpoints, AttributeType.Overshield, newValue, currentValue, delta);
                break;

            case AttributeType.CurrentPower:
                newValue = ApplyCap(AttributeType.MaxPower, AttributeType.PowerSurge, newValue, currentValue, delta);
                break;
        }
        return newValue;
    }

    float ApplyCap(AttributeType attributeTypeMax, AttributeType attributeTypeTempMax, float newValue, float currentValue, float delta)
    {
        float max = GetAttribute(attributeTypeMax);
        float tempMax = GetAttribute(attributeTypeTempMax);
        if (newValue > max)
            return max;

        if (delta < 0 && tempMax > 0)
        {
            if (tempMax + delta <= 0)
            {
                SetAttribute(attributeTypeTempMax, 0);
                return currentValue + tempMax + delta;
            }
            else
                SetAttribute(attributeTypeTempMax, tempMax + delta);
            return currentValue;
        }
        return newValue;
    }
    #endregion

    #region Handle Mana Regeneration
    private void RegenerateMana()
    {
        //if (GetAttribute(AttributeType.CurrentHitpoints) < GetAttribute(AttributeType.MaxHitpoints))
        //    ChangeAttribute(AttributeType.CurrentHitpoints, HealthRegeneration);
        int calculatedManaRegen = CalaculateManaRegen();
        if (GetAttribute(AttributeType.CurrentPower) < GetAttribute(AttributeType.MaxPower))
            ChangeAttribute(AttributeType.CurrentPower, calculatedManaRegen);
    }
    private int CalaculateManaRegen()
    {
        return (int)PowerRegeneration;
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

    #endregion
    protected void Die()
    {
        Logging.Info($"{gameObject.name} died");
        if (gameObject.GetComponent<MinionStats>() != null) EventDeclarer.minionDied?.Invoke(this);
        Destroy(gameObject);
    }
}
