using System;
using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;

public class PlayerStats : LivingBeing
{
    public static PlayerStats Instance;
    [SerializeField] public int CurrentLevel { private set; get; } = 0;
    [SerializeField] public int CurrentXP { private set; get; } = 0;
    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }
}
