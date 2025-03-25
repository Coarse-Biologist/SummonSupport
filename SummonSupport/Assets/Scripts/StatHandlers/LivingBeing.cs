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


    void Start()
    {

    }
    void FixedUpdate()
    {

    }
}
