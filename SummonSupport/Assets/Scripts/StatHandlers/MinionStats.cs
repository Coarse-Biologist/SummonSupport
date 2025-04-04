using UnityEngine;
using System.Collections.Generic;
using Alchemy;

public class MinionStats : LivingBeing
{
    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability
    [SerializeField] public List<AlchemyLoot> Loot { private set; get; } = new List<AlchemyLoot>();

    [SerializeField] public MinionCommands CurrentCommand { private set; get; } = MinionCommands.None;


    new public void AlterHP(int value)
    {
        CurrentHP += value;
        if (CurrentHP <= 0) isDead = true;
        if (CurrentHP > MaxHP) CurrentHP = MaxHP;

        ED.hpChanged?.Invoke(gameObject);
    }
    public void SetCommand(MinionCommands command)
    {
        CurrentCommand = command;
    }

}


public enum MinionCommands
{
    None,
    GoTo,
    FocusTarget,

}
