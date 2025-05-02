using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using SummonSupportEvents;
using Unity.Entities.UniversalDelegates;
public abstract class LivingBeing : MonoBehaviour
{
    #region Declarations

    [Header("String Stats")]
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public List<string> BattleCries { get; private set; }

    [Header("Attributes - Ressources")]
    [field: SerializeField] public float MaxHP { get; private set; } = 100;
    [field: SerializeField] public float TemporaryMaxHP { get; private set; }
    [field: SerializeField] public float CurrentHP { get; private set; } = 100;
    [field: SerializeField] public float MaxPower { get; private set; } = 100;
    [field: SerializeField] public float TemporaryMaxPower { get; private set; }
    [field: SerializeField] public float CurrentPower { get; private set; } = 100;

    [Header("Attributes - Regenerations")]
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

    [Header("Other")]
    [field: SerializeField] public float XP_OnDeath { get; private set; } = 3;
    public Dictionary<string, StatusEffectInstance> activeStatusEffects = new();
    public Dictionary<Element, (Func<float> Get, Action<float> Set)> Affinities { private set; get; } = new();
    public Dictionary<AttributeType, (Func<float> Get, Action<float> Set)> AttributesDict { private set; get; } = new();

    [field: SerializeField] public List<Ability> Abilties { get; private set; } = new();
    [field: SerializeField] public float Speed { get; private set; } = 0.4f;
    [field: SerializeField] public float Mass { get; private set; } = 1f;

    #endregion Declarations

    #region Color control
    public Dictionary<RGBAEnum, (Func<float> Get, Action<float> Set)> ColorDict { private set; get; } = new();
    private float redness;
    private float greeness;
    private float blueness;
    private float alphaness;
    //private float[] rgbaValues = new float[4] { redness, greeness, blueness, alphaness };

    private SpriteRenderer spriteRenderer;

    [SerializeField] public I_HealthBar healthbarInterface;

    #endregion

    protected void Awake()
    {
        GetComponent<Rigidbody2D>().mass = Mass;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        InitializeAttributeDict();
        InitializeAffinityDict();
        InitializeColorDict();
        healthbarInterface = GetComponent<I_HealthBar>();
    }


    void Start()
    {
        //healthbarInterface = GetComponent<I_HealthBar>();
        //if (healthbarInterface == null) Logging.Info("health interface not found in start");
        //else Logging.Info("healthbar interface found in start");
        //
        //if (healthbarInterface != null)
        //{
        //    Logging.Info("health bar interface  exists!!! Well done, start function!");
        //    healthbarInterface.SetHealthBarValue(GetAttribute(AttributeType.CurrentHitpoints));
        //    healthbarInterface.SetHealthBarMaxValue(GetAttribute(AttributeType.MaxHitpoints));
        //}
        //else Logging.Info("health bar interface doesnt exist!!! Good try, start function");

    }


    #region Stat Upgrades

    public void ChangeMaxHP(float amount)
    {
        MaxHP += amount;
    }
    public void ChangeMaxPower(float amount)
    {
        MaxPower += amount;
        Logging.Info($"power increased by {amount}");
        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(3, 3);
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

    #region Resource Control

    public void RestoreResources()
    {
        CurrentHP = MaxHP;
        CurrentPower = MaxPower;
    }

    public void ReduceHP(float amount)
    {
        CurrentHP -= amount;
    }

    #endregion
    public void GainAffinity(Element element, float amount)
    {
        Logging.Info($"gain element function has been called");
        if (Affinities.TryGetValue(element, out (Func<float> Get, Action<float> Set) func))
        {
            Affinities[element].Set(amount + Affinities[element].Get());
        }
    }

    #region Affinity handling
    public void SetColor(float[] rgbaValues)
    {
        float r = rgbaValues[0];
        float g = rgbaValues[1];
        float b = rgbaValues[2];
        float a = rgbaValues[3];
        spriteRenderer.color = new Color(r, g, b, a);
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
    public void SetName(string newName)
    {
        Name = newName;
    }

    public float ChangeAttribute(AttributeType attributeType, float value, ValueType valueType = ValueType.Flat)
    {
        if (AttributesDict == null || !AttributesDict.ContainsKey(attributeType))
            throw new Exception("Attribute not found or invalid setter");

        float currentValue = GetAttribute(attributeType);
        float newValue = CalculateNewValueByType(currentValue, value, valueType);
        SetAttribute(attributeType, newValue);

        if (IsDead())
            Die();
        return newValue;
    }

    public bool IsDead()
    {
        return CurrentHP <= 0;
    }

    public void HandleEventInvokes(AttributeType attributeType, float newValue)
    {
        switch (attributeType)
        {
            case AttributeType.CurrentHitpoints:
            case AttributeType.CurrentPower:
                EventDeclarer.attributeChanged?.Invoke(this, attributeType);

                if (healthbarInterface != null)
                {
                    healthbarInterface.SetHealthBarValue(GetAttribute(AttributeType.CurrentHitpoints));
                }
                break;
            case AttributeType.MaxHitpoints:
            case AttributeType.MaxPower:
                EventDeclarer.maxAttributeChanged?.Invoke();
                if (healthbarInterface != null)
                {
                    healthbarInterface.SetHealthBarValue(GetAttribute(AttributeType.MaxHitpoints));
                }
                break;

            case AttributeType.MovementSpeed:
            case AttributeType.DashBoost:
            case AttributeType.DashCooldown:
            case AttributeType.DashDuration:
                EventDeclarer.SpeedAttributeChanged.Invoke(attributeType, newValue);
                break;
        }
    }

    float CalculateNewValueByType(float currentValue, float value, ValueType valueType)
    {
        if (valueType == ValueType.Percentage)
            value = currentValue * (1 + (value / 100f));
        else // value type is flat
            value = currentValue + value;
        return value;
    }
    #endregion
    #region color control
    public void AlterColorByAffinity()
    {
        Logging.Info($"{Affinities.Keys.Count}");
        Element strongestElement = Affinities.OrderByDescending(a => a.Value.Get()).First().Key;

        string str = strongestElement.ToString();
        string nameBase = "Elemental";
        SetName(strongestElement.ToString() + nameBase);
        if (Affinities[strongestElement].Get() > 50)
        {
            if (str.Contains("Cold") || str.Contains("Water"))
            {
                SetColor(new float[4] { 0f, 0f, 1f, 1f });
            }
            if (str.Contains("Plant") || str.Contains("Bacteria"))
            {
                SetColor(new float[4] { 0f, 1f, 0f, 1f });
            }
            if (str.Contains("Virus") || str.Contains("Acid"))
            {
                SetColor(new float[4] { 0.9f, 0.7f, 0.0f, 1.0f });
            }
            if (str.Contains("Light") || str.Contains("Electricity"))
            {
                SetColor(new float[4] { 0.85f, 0.85f, 0.0f, 1.0f });
            }
            if (str.Contains("Heat") || str.Contains("Radiation"))
            {
                SetColor(new float[4] { 1f, 0f, 0.0f, 1.0f });
            }
            if (str.Contains("Psychic") || str.Contains("Poison"))
            {
                SetColor(new float[4] { 0.5f, 0f, .5f, 1.0f });
            }
            if (str.Contains("Fungi") || str.Contains("Earth"))
            {
                SetColor(new float[4] { .4f, 0.4f, .4f, 1.0f });
            }
        }
        else Logging.Info($"{strongestElement} has less than 50");
    }
    public void SlideColor(RGBAEnum color, float range)
    {
        ColorDict[color].Set(range);
        SetColor(new float[] { redness, greeness, blueness, alphaness });
    }
    public void InitializeColorDict()
    {
        ColorDict = new Dictionary<RGBAEnum, (Func<float> Get, Action<float> Set)>
            {
                { RGBAEnum.Red,           (() => redness,               v => redness = v) },
                { RGBAEnum.Green,           (() => greeness,               v => greeness = v) },
                { RGBAEnum.Blue,           (() => blueness,               v => blueness = v) },
                { RGBAEnum.Alpha,           (() => alphaness,               v => alphaness = v) },
            };
    }
    public enum RGBAEnum
    {
        Red,
        Green,
        Blue,
        Alpha
    }
    #endregion

    void InitializeAttributeDict()
    {
        AttributesDict = new Dictionary<AttributeType, (Func<float> Get, Action<float> Set)>
            {
                { AttributeType.MaxHitpoints,           (() => MaxHP,               v => MaxHP = v) },
                { AttributeType.TemporaryMaxHitpoints,  (() => TemporaryMaxHP,      v => TemporaryMaxHP = v) },
                { AttributeType.CurrentHitpoints,       (() => CurrentHP,           v => CurrentHP = v) },
                { AttributeType.MaxPower,               (() => MaxPower,            v => MaxPower = v) },
                { AttributeType.TemporaryMaxPower,      (() => TemporaryMaxPower,   v => TemporaryMaxPower = v) },
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
    protected void Die()
    {
        Logging.Info($"{gameObject.name} died");
        if (gameObject.GetComponent<MinionStats>() != null) EventDeclarer.minionDied?.Invoke(this);
        Destroy(gameObject);
    }
}
