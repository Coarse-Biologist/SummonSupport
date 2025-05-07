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
    void Start()
    {
        StartCoroutine("LateStart");
        
        healthbarInterface = GetComponent<I_HealthBar>();

        if (healthbarInterface != null)
        {
            healthbarInterface.SetHealthBarMaxValue(GetAttribute(AttributeType.MaxHitpoints));
            healthbarInterface.SetHealthBarValue(GetAttribute(AttributeType.CurrentHitpoints));
        }
        else Logging.Info($"{Name} has no health interface yet");
    }
    IEnumerator LateStart()
    {
        yield return null; // wartet 1 Frame – nach allen Start()-Methoden
        PlayerUIHandler.Instance.AddMinionHP(this);
    }

}

public enum MinionCommands
{
    None,
    GoTo,
    FocusTarget,
    Interact,
}
