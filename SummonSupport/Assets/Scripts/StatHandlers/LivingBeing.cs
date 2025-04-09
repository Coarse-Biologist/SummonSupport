using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Alchemy;
using System;
using UnityEngine;
using NUnit.Framework;

public abstract class LivingBeing : MonoBehaviour
{
    #region declarations
    [Header("String Stats")]
    [SerializeField] public string Name;
    [SerializeField] public string Description;
    [SerializeField] public List<string> BattleCries;

    [Header("Attributes - Ressources")]
    [field: SerializeField] public int MaxHP { get; private set; }
    [field: SerializeField] public int TemporaryMaxHP { get; private set; }
    [field: SerializeField] public int CurrentHP { get; private set; }
    [field: SerializeField] public int MaxPower { get; private set; }
    [field: SerializeField] public int TemporaryMaxPower { get; private set; }
    [field: SerializeField] public int CurrentPower { get; private set; }

    #region Affinity Stats

    [Header("Affinity Stats")]
    [SerializeField] protected int Cold = 0;
    [SerializeField] protected int Water = 0;
    [SerializeField] protected int Earth = 0;
    [SerializeField] protected int Heat = 0;
    [SerializeField] protected int Air = 0;
    [SerializeField] protected int Electricity = 0;
    [SerializeField] protected int Poison = 0;
    [SerializeField] protected int Acid = 0;
    [SerializeField] protected int Bacteria = 0;
    [SerializeField] protected int Fungi = 0;
    [SerializeField] protected int Plant = 0;
    [SerializeField] protected int Virus = 0;
    [SerializeField] protected int Radiation = 0;
    [SerializeField] protected int Light = 0;
    [SerializeField] protected int Psychic = 0;
    #endregion

    [Header("Other")]
    public List<string> statusEffects = new List<string>();
    public int XP_OnDeath = 3;
    protected bool isDead = false;
    public Dictionary<Elements, (Func<int> Get, Action<int> Set)> Affinities { private set; get; } = new Dictionary<Elements, (Func<int> Get, Action<int> Set)>();
    public Dictionary<AttributeType, (Func<int> Get, Action<int> Set)> AttributesDict { private set; get; } = new Dictionary<AttributeType, (Func<int> Get, Action<int> Set)>();

    [SerializeField] public List<Ability> Abilties = new List<Ability>();
    [SerializeField] public float Speed;
    [SerializeField] public float Mass = 1;


    private enum AccessType
    {
        Getter,
        Setter
    }
    #region Color control
    public Dictionary<RGBAEnum, (Func<float> Get, Action<float> Set)> ColorDict { private set; get; } = new Dictionary<RGBAEnum, (Func<float> Get, Action<float> Set)>();
    private float redness;
    private float greeness;
    private float blueness;
    private float alphaness;
    //private float[] rgbaValues = new float[4] { redness, greeness, blueness, alphaness };

    private SpriteRenderer spriteRenderer;

    #endregion
    protected EventDeclarer ED;

    protected void Awake()
    {
        GetComponent<Rigidbody2D>().mass = Mass;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        InitializeAttributeDict();
        InitializeAffinityDict();
        InitializeColorDict();
        Logging.Info($"{Affinities.Keys.Count}");
        ED = FindFirstObjectByType<EventDeclarer>();
    }

    Dictionary<AttributeType, Dictionary<AccessType, Delegate>> dictAttributes;
    #endregion

    #region Stat Upgrades

    public void ChangeMaxHP(int amount)
    {
        MaxHP += amount;
    }
    public void ChangeMaxPower(int amount)
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
    public void ChangeSpeed(int amount)
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

    public void ReduceHP(int amount)
    {
        CurrentHP -= amount;
    }

    #endregion
    public void GainAffinity(Elements element, int amount)
    {
        Logging.Info($"gain element function has been called");
        if (Affinities.TryGetValue(element, out (Func<int> Get, Action<int> Set) func))
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

    public int GetAttribute(AttributeType attribute)
    {
        if (AttributesDict != null && AttributesDict.ContainsKey(attribute))
        {
            return AttributesDict[attribute].Get();
        }
        throw new Exception("Attribute not found");

    }

    public void SetAttribute(AttributeType attribute, int value)
    {
        if (AttributesDict != null && AttributesDict.ContainsKey(attribute))
        {
            AttributesDict[attribute].Set(value);
        }
    }
    public void SetName(string newName)
    {
        Name = newName;
    }

    public void ChangeAttribute(AttributeType attributeType, int value, ValueType valueType = ValueType.Flat)
    {
        if (valueType == ValueType.Percentage)
            value = Mathf.RoundToInt(GetAttribute(attributeType) * (1 + (value / 100f)));
        else if (valueType == ValueType.Flat)
            value = GetAttribute(attributeType) + value;

        if (AttributesDict != null && AttributesDict.ContainsKey(attributeType))
        {
            AttributesDict[attributeType].Set(value);
        }
        else
        {
            throw new Exception("Attribute not found or invalid setter");
        }
        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    #endregion
    #region color control
    public void AlterColorByAffinity()
    {
        Logging.Info($"{Affinities.Keys.Count}");
        Elements strongestElement = Affinities.OrderByDescending(a => a.Value.Get()).First().Key;

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

    public void InitializeAttributeDict()
    {
        AttributesDict = new Dictionary<AttributeType, (Func<int> Get, Action<int> Set)>
            {
                { AttributeType.MaxHitpoints,           (() => MaxHP,               v => MaxHP = v) },
                { AttributeType.TemporaryMaxHitpoints,  (() => TemporaryMaxHP,      v => TemporaryMaxHP = v) },
                { AttributeType.CurrentHitpoints,       (() => CurrentHP,           v => CurrentHP = v) },
                { AttributeType.MaxPower,               (() => MaxPower,            v => MaxPower = v) },
                { AttributeType.TemporaryMaxPower,      (() => TemporaryMaxPower,   v => TemporaryMaxPower = v) },
                { AttributeType.CurrentPower,           (() => CurrentPower,        v => CurrentPower = v) },
            };
    }

    public void InitializeAffinityDict()
    {
        Affinities = new Dictionary<Elements, (Func<int> Get, Action<int> Set)>
            {
                { Elements.Cold,            (() => Cold,            v => Cold = v) },
                { Elements.Water,           (() => Water,           v => Water = v) },
                { Elements.Earth,           (() => Earth,           v => Earth = v) },
                { Elements.Heat,            (() => Heat,            v => Heat = v) },
                { Elements.Air,             (() => Air,             v => Air = v) },
                { Elements.Electricity,     (() => Electricity,     v => Electricity = v) },
                { Elements.Poison,          (() => Poison,          v => Poison = v) },
                { Elements.Acid,            (() => Acid,            v => Acid = v) },
                { Elements.Bacteria,        (() => Bacteria,        v => Bacteria = v) },
                { Elements.Fungi,           (() => Fungi,           v => Fungi = v) },
                { Elements.Plant,           (() => Plant,           v => Plant = v) },
                { Elements.Virus,           (() => Virus,           v => Virus = v) },
                { Elements.Radiation,       (() => Radiation,       v => Radiation = v) },
                { Elements.Light,           (() => Light,           v => Light = v) },
                { Elements.Psychic,         (() => Psychic,         v => Psychic = v) }
            };
    }
    public void Die()
    {
        Logging.Info($"{gameObject.name} died");
        Destroy(gameObject);
    }
}
