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
        ToggleDeath(true);
    }

    private void ToggleDeath(bool dead)
    {
        if (dead)
        {
            gameObject.AddComponent<I_InteractMinionResurrect>();
            if (TryGetComponent<Collider>(out Collider collider))
            {
                collider.isTrigger = true;
            }
            if (gameObject.TryGetComponent<AIStateHandler>(out AIStateHandler stateHandler))
            {
                stateHandler.SetDead(true);
            }
        }
        else
        {
            if (gameObject.TryGetComponent<I_InteractMinionResurrect>(out I_InteractMinionResurrect resurrectScript))
                Destroy(resurrectScript);
            if (TryGetComponent<Collider>(out Collider collider))
            {
                collider.isTrigger = false;
            }
            if (gameObject.TryGetComponent<AIStateHandler>(out AIStateHandler stateHandler))
            {
                stateHandler.SetDead(false);
                Debug.Log("Setting state handler death to false");
            }

        }
    }

    public void Resurrect()
    {
        Debug.Log($"Resurrecting minion {Name}");
        ToggleDeath(false);
        ChangeAttribute(AttributeType.CurrentHitpoints, 50f);
    }

}


public enum MinionCommands
{
    None,
    GoTo,
    FocusTarget,
    Interact,
}
