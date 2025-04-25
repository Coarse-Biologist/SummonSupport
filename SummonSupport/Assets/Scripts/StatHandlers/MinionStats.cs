using UnityEngine;
using System.Collections.Generic;
using Alchemy;
using SummonSupportEvents;

public class MinionStats : LivingBeing
{
    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability
    [SerializeField] public List<AlchemyLoot> Loot { private set; get; } = new List<AlchemyLoot>();
    [SerializeField] public MinionCommands CurrentCommand { private set; get; } = MinionCommands.None;

    public void SetMinionHP(int value)
    {
        SetAttribute(AttributeType.CurrentHitpoints, value);
        if (CurrentHP <= 0) isDead = true;
        if (CurrentHP > MaxHP) SetAttribute(AttributeType.MaxHitpoints, value);
    }
    public void SetCommand(MinionCommands command)
    {
        CurrentCommand = command;
    }
    void Start()
    {
        PlayerUIHandler.Instance.AddMinionHP(this.gameObject);
    }

}

public enum MinionCommands
{
    None,
    GoTo,
    FocusTarget,
    Interact,
}
