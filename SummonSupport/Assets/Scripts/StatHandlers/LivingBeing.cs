using System.Collections.Generic;
using System.Linq;
using Alchemy;
using UnityEngine;
using System;

public abstract class LivingBeing : MonoBehaviour
{
    [Header("String Stats")]
    [SerializeField] public string Name;
    [SerializeField] public string Description;
    [SerializeField] public List<string> BattleCries;

    [Header("Resource Stats")]
    [SerializeField] public int MaxHP;
    [SerializeField] public int CurrentHP;
    [SerializeField] public int MaxPower;
    [SerializeField] public int CurrentPower;

    #region

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
    public int XP_OnDeath = 3;


    public Dictionary<Elements, (Func<int> Get, Action<int> Set)> Affinities { private set; get; } = new Dictionary<Elements, (Func<int> Get, Action<int> Set)>();

    [SerializeField] public List<string> Abilties = new List<string>();
    // placeholder while we see what form the abilities will take
    [SerializeField] public float Speed;
    [SerializeField] public float Mass = 1;

    private float[] rgbaValues = new float[4] { 0f, 0f, 0f, 0f };
    private SpriteRenderer spriteRenderer;


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
        Affinities[element].Set(amount + Affinities[element].Get());
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



    public void AlterColorByAffinity()
    {
        Elements strongestElement = Affinities.OrderByDescending(a => a.Value.Get()).First().Key;

        string str = strongestElement.ToString();
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
            if (str.Contains("Light") || str.Contains("Radiation"))
            {
                SetColor(new float[4] { 0.85f, 0.85f, 0.0f, 1.0f });
            }
        }
        else Logging.Info($"{strongestElement} has less than 50");
    }


    public void SetAffinityDict()
    {
        Affinities = new Dictionary<Elements, (Func<int> Get, Action<int> Set)>
            {
                { Elements.Cold, (() => Cold, v => Cold = v) },
                { Elements.Water, (() => Water, v => Water = v) },
                { Elements.Earth, (() => Earth, v => Earth = v) },
                { Elements.Heat, (() => Heat, v => Heat = v) },
                { Elements.Air, (() => Air, v => Air = v) },
                { Elements.Electricity, (() => Electricity, v => Electricity = v) },
                { Elements.Poison, (() => Poison, v => Poison = v) },
                { Elements.Acid, (() => Acid, v => Acid = v) },
                { Elements.Bacteria, (() => Bacteria, v => Bacteria = v) },
                { Elements.Fungi, (() => Fungi, v => Fungi = v) },
                { Elements.Plant, (() => Plant, v => Plant = v) },
                { Elements.Virus, (() => Virus, v => Virus = v) },
                { Elements.Radiation, (() => Radiation, v => Radiation = v) },
                { Elements.Light, (() => Light, v => Light = v) },
                { Elements.Psychic, (() => Psychic, v => Psychic = v) }
            };
    }

    #endregion
    void Awake()
    {
        GetComponent<Rigidbody2D>().mass = Mass;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        SetAffinityDict();
    }
    void FixedUpdate()
    {

    }
}
