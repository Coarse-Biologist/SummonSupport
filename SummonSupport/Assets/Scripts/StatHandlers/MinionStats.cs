using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using SummonSupportEvents;
using System;

public class MinionStats : LivingBeing
{
    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability
    [SerializeField] public List<AlchemyLoot> Loot { private set; get; } = new List<AlchemyLoot>();
    [SerializeField] public MinionCommands CurrentCommand { private set; get; } = MinionCommands.None;

    //private I_HealthBar healthbarInterface;

    public override void HandleUIAttrDisplay(AttributeType attributeType, float newValue)
    {
        PlayerStats.Instance.UiHandler.UpdateResourceBar(this, attributeType);
        switch (attributeType)
        {
            case AttributeType.MaxHitpoints:
                resourceBarInterface?.SetHealthBarMaxValue(GetAttribute(attributeType));
                break;

            case AttributeType.CurrentHitpoints:
                resourceBarInterface?.SetHealthBarValue(GetAttribute(attributeType));
                break;

            case AttributeType.MaxPower:
                resourceBarInterface?.SetPowerBarMaxValue(GetAttribute(attributeType));
                break;

            case AttributeType.CurrentPower:
                resourceBarInterface?.SetPowerBarValue(GetAttribute(attributeType));
                break;
            default:
                break;

        }
    }

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
        CommandMinion.AddActiveMinions(this);
        ColorChanger.ChangeAllMatsByAffinity(this);
        base.Start();


    }

    public override void Die()
    {
        EventDeclarer.minionDied?.Invoke(gameObject);
        SetRegeneration(AttributeType.CurrentHitpoints, 0);
        if (ragdollScript != null) ragdollScript.CauseDestruction(true);
        ToggleDeath(true);
    }
    private void DelayedTestDeath()
    {
        Debug.Log("Delayed test death happening now");
        Destroy(gameObject);
    }

    private void ToggleDeath(bool dead)
    {
        if (dead)
        {
            gameObject.AddComponent<I_InteractMinionResurrect>();
            if (TryGetComponent(out Collider collider))
            {
                collider.isTrigger = true;
            }
            if (gameObject.TryGetComponent(out AIStateHandler stateHandler))
            {
                stateHandler.SetDead(true);
            }
        }
        else
        {
            if (gameObject.TryGetComponent(out I_InteractMinionResurrect resurrectScript))
                Destroy(resurrectScript);
            if (TryGetComponent(out Collider collider))
            {
                collider.isTrigger = false;
            }
            if (gameObject.TryGetComponent(out AIStateHandler stateHandler))
            {
                stateHandler.SetDead(false);
                Debug.Log("Setting state handler death to false");
                //stateHandler.StartCoroutine(stateHandler.RunStateMachine());
            }

        }
    }

    public void Resurrect()
    {
        Debug.Log($"Resurrecting minion {Name}");
        ToggleDeath(false);
        ChangeAttribute(AttributeType.CurrentHitpoints, 50f);
        if (ragdollScript != null) ragdollScript.ReverseDestruction();
    }

    public void ApplyStatusEffect(StatusEffectType status, bool apply)
    {
        //throw new NotImplementedException();
    }
}


public enum MinionCommands
{
    None,
    GoTo,
    FocusTarget,
    Interact,
}
