using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using SummonSupportEvents;

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

    public override void Die()
    {
        Logging.Info($"{gameObject.name} died");
        EventDeclarer.minionDied?.Invoke(gameObject);
        if (HasStatusEffect(StatusEffectType.ExplodeOnDeath)) ViciousDeathExplosion();
        //Destroy(gameObject);
        gameObject.AddComponent<I_InteractMinionResurrect>();
        if (TryGetComponent<Collider2D>(out Collider2D collider))
        {
            collider.isTrigger = true;
            Debug.Log("Dying and therefore giving self resurrect interactable and setttin collider to trigger");
        }
    }
    public void Resurrect()
    {
        Debug.Log($"Resurrecting minion {Name}");
    }

}


public enum MinionCommands
{
    None,
    GoTo,
    FocusTarget,
    Interact,
}
