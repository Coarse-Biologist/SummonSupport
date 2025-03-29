using System.Collections.Generic;
using Alchemy;
using UnityEngine;

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
    void Awake()
    {
        GetComponent<Rigidbody2D>().mass = Mass;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    void FixedUpdate()
    {

    }
}
