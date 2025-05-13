using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class MinionStats : LivingBeing
{
    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability
    [SerializeField] public List<AlchemyLoot> Loot { private set; get; } = new List<AlchemyLoot>();
    [SerializeField] public MinionCommands CurrentCommand { private set; get; } = MinionCommands.None;
    //private I_HealthBar healthbarInterface;


    public void SetCommand(MinionCommands command)
    {
        CurrentCommand = command;
    }
    protected override void Start()
    {
        StartCoroutine("LateStart");
    }
    IEnumerator LateStart()
    {
        yield return null; // wartet 1 Frame – nach allen Start()-Methoden
        PlayerUIHandler.Instance.AddMinionHP(this);
        base.Start();
    }

}

public enum MinionCommands
{
    None,
    GoTo,
    FocusTarget,
    Interact,
}
