using UnityEngine;
using System.Collections.Generic;
using Alchemy;

public class MinionStats : LivingBeing
{
    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability
    [SerializeField] List<AlchemyLoot> Loot = new List<AlchemyLoot>();

    [SerializeField] public MinionCommands CurrentCommand = MinionCommands.None;

    private EventDeclarer ED;

    new public void AlterHP(int value)
    {
        CurrentHP -= value;
        ED.hpChanged?.Invoke(gameObject);
    }
    public void SetCommand(MinionCommands command)
    {
        CurrentCommand = command;
    }
    void Awake()
    {
        ED = FindFirstObjectByType<EventDeclarer>();

    }
}


public enum MinionCommands
{
    None,
    GoTo,
    FocusTarget,

}
