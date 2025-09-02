using System;
using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;

public class PlayerStats : LivingBeing
{
    public static PlayerStats Instance;

    [Header("Experience Info")]
    [SerializeField] public int CurrentLevel { private set; get; } = 0;
    [SerializeField] public float CurrentXP { private set; get; } = 0;
    [SerializeField] public float MaxXP { private set; get; } = 10;

    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    void OnEnable()
    {
        EventDeclarer.EnemyDefeated?.AddListener(GainXP);
    }
    void OnDisable()
    {
        EventDeclarer.EnemyDefeated?.RemoveListener(GainXP);

    }
    private void GainXP(LivingBeing defeatedEnemy)
    {
        CurrentXP += defeatedEnemy.XP_OnDeath;
        if (CurrentXP >= MaxXP)
        {
            LevelUp();
        }
        PlayerUIHandler.Instance.SetPlayerXP(CurrentXP);
    }
    public void GainXP(int amount)
    {
        CurrentXP += amount;
        if (CurrentXP >= MaxXP)
        {
            LevelUp();
        }
        PlayerUIHandler.Instance.SetPlayerXP(CurrentXP);
    }

    private void LevelUp()
    {
        CurrentLevel += 1;
        CurrentXP -= MaxXP;
        MaxXP *= 2;
    }
}
