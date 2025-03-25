using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : LivingBeing
{
    [SerializeField] public int CurrentLevel { private set; get; } = 0;
    [SerializeField] public int CurrentXP { private set; get; } = 0;
    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability


    void Start()
    {

    }

    void Update()
    {

    }
}
