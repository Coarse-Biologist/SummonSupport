using System.Collections.Generic;
using System.Runtime.InteropServices;
using Alchemy;
using System;
using UnityEngine;


public abstract class LivingBeing : MonoBehaviour
{
    #region declarations
    [Header("String Stats")]
    [SerializeField] public string Name;
    [SerializeField] public string Description;
    [SerializeField] public List<string> BattleCries;
    
    [Header("Attributes - Ressources")]
    [field: SerializeField] public int MaxHP               { get; private set; }
    [field: SerializeField] public int TemporaryMaxHP      { get; private set; }
    [field: SerializeField] public int CurrentHP           { get; private set; }
    [field: SerializeField] public int MaxPower            { get; private set; }
    [field: SerializeField] public int TemporaryMaxPower   { get; private set; }
    [field: SerializeField] public int CurrentPower        { get; private set; }

    [Header("Other")]
    public int XPonDeath = 3;
    public Dictionary<Elements, int> Affinities { private set; get; } = new Dictionary<Elements, int>
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
    [SerializeField] public List<Ability> Abilties = new List<Ability>();
    [SerializeField] public float Speed;
    [SerializeField] public float Mass = 1;

    private enum AccessType
    {
        Getter,
        Setter
    }


    private float[] rgbaValues = new float[4] { 0f, 0f, 0f, 0f };
    private SpriteRenderer spriteRenderer;

    Dictionary<AttributeType, Dictionary<AccessType, Delegate>> dictAttributes;
    #endregion
    
    void Awake()
    {
        GetComponent<Rigidbody2D>().mass = Mass;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected void Start()
    {
        Logging.Info("Starting Living being");
        InitializeAttributeDictionary();
    }

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

    public void GainAffinity(Elements element, int amount)
    {
        Affinities[element] += amount;
    }


    #endregion

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
    void InitializeAttributeDictionary()
    {
        Logging.Info("Initializing Attribute Dictionary!");
        dictAttributes = new Dictionary<AttributeType, Dictionary<AccessType, Delegate>>
        {
            { AttributeType.MaxHitpoints, new Dictionary<AccessType, Delegate>
                {
                    { AccessType.Getter, new Func<int>  (()         => MaxHP) },
                    { AccessType.Setter, new Action<int>((value)    => MaxHP = value) }
                }
            },
            { AttributeType.CurrentHitpoints, new Dictionary<AccessType, Delegate>
                {
                    { AccessType.Getter, new Func<int>  (()         => CurrentHP) },
                    { AccessType.Setter, new Action<int>((value)    => CurrentHP = value) }
                }
            },
            { AttributeType.TemporaryMaxHitpoints, new Dictionary<AccessType, Delegate>
                {
                    { AccessType.Getter, new Func<int>  (()         => TemporaryMaxHP) },
                    { AccessType.Setter, new Action<int>((value)    => TemporaryMaxHP = value) }
                }
            },
            { AttributeType.MaxPower, new Dictionary<AccessType, Delegate>
                {
                    { AccessType.Getter, new Func<int>  (()         => MaxPower) },
                    { AccessType.Setter, new Action<int>((value)    => MaxPower = value) }
                }
            }
        };
        Debug.Log("Attribute Dictionary Initialized!");
    }
    public int GetAttribute(AttributeType attributeType)
    {
        if (dictAttributes.ContainsKey(attributeType) && dictAttributes[attributeType][AccessType.Getter] is Func<int> getter)
        {
            return getter();
        }
        throw new Exception("Attribute not found");
    }

    public void SetAttribute(AttributeType attributeType, int value)
    {
        if (dictAttributes.ContainsKey(attributeType) && dictAttributes[attributeType][AccessType.Setter] is Action<int> setter)
        {
            setter(value);
        }
        else
        {
            throw new Exception("Attribute not found or invalid setter");
        }
    }
    public void ChangeAttribute(AttributeType attributeType, int value, ValueType valueType = ValueType.Flat)
    {
        Logging.Info("Change Attribute: " + attributeType);
        Logging.Info("Dict: "+ dictAttributes);
        if (valueType == ValueType.Percentage)
            value = Mathf.RoundToInt(GetAttribute(attributeType) * (1 + (value / 100f)));
        else if (valueType == ValueType.Flat)
            value = GetAttribute(attributeType) + value;
        Logging.Info("Changed Value: " + value);

        if (dictAttributes.ContainsKey(attributeType) && dictAttributes[attributeType][AccessType.Setter] is Action<int> setter)
        {
            setter(value);
        }
        else
        {
            throw new Exception("Attribute not found or invalid setter");
        }
    }
    #endregion
}
