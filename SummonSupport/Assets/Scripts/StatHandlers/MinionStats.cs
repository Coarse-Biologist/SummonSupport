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
    /// <summary>How much crafting has grown the minion, relative to its prefab size.</summary>
    public float BodyScale { private set; get; } = 1f;
    private Vector3 prefabScale;

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
        base.Start();
        StartCoroutine(LateStart());
    }
    protected override void Awake()
    {
        base.Awake();
        prefabScale = transform.localScale;
    }
    IEnumerator LateStart()
    {
        yield return null; // wartet 1 Frame – nach allen Start()-Methoden
        ColorChanger.ChangeAllMatsByAffinity(this);

        // Online, each player only controls (and sees the health bars of) the minions they crafted.
        if (!NetworkEnemy.IsLocalPlayersMinion(this)) yield break;
        EventDeclarer.newMinionAdded?.Invoke(this);
        PlayerUIHandler.Instance.AddMinionHP(this);
    }

    /// <summary>Grows the minion by a factor, for example when it is crafted with organs.</summary>
    public void ScaleBody(float factor)
    {
        SetBodyScale(BodyScale * factor);
    }

    public void SetBodyScale(float scale)
    {
        if (scale <= 0) return;
        BodyScale = scale;
        transform.localScale = prefabScale * scale;
    }

    /// <summary>Applies the maximums its crafter decided, without the checks that setting them locally runs.</summary>
    public void ApplyReplicatedMaximums(float maxHitpoints, float maxPower)
    {
        if (maxHitpoints > 0 && ResourceAttributesDict.TryGetValue(AttributeType.MaxHitpoints, out var maxHp))
        {
            maxHp.Set(maxHitpoints);
            HandleUIAttrDisplay(AttributeType.MaxHitpoints, maxHitpoints);
        }
        if (maxPower > 0 && ResourceAttributesDict.TryGetValue(AttributeType.MaxPower, out var maxPow))
        {
            maxPow.Set(maxPower);
            HandleUIAttrDisplay(AttributeType.MaxPower, maxPower);
        }
    }

    public override void RestoreResources()
    {
        base.RestoreResources();
        // Online, the host's hitpoints and power are the real ones, so it has to fill them too.
        if (NetworkSession.IsOnline && TryGetComponent(out NetworkEnemy networkEnemy)) networkEnemy.RequestRestoreResources();
    }

    public override void Die()
    {
        if (Dead) return;
        Dead = true;
        abilityHandler.StopAllToggledAbilities();
        EventDeclarer.minionDied?.Invoke(gameObject);
        SetRegeneration(AttributeType.CurrentHitpoints, 0);
        if (ragdollScript != null) ragdollScript.CauseDestruction(true);
        ToggleDeath(true);
    }
    public void TrueDeath()
    {
        EventDeclarer.minionDied?.Invoke(gameObject);
        AlchemyHandler.Instance.RemoveActiveMinion(this);
        Destroy(gameObject, .1f);
    }

    private void ToggleDeath(bool dead)
    {
        if (dead)
        {
            gameObject.AddComponent<I_InteractMinionResurrect>();
            if (TryGetComponent(out BoxCollider collider))
            {
                collider.isTrigger = true;
                collider.size *= 3f;
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
            if (TryGetComponent(out BoxCollider collider))
            {
                collider.isTrigger = false;
                collider.size /= 3f;

            }
            if (gameObject.TryGetComponent(out AIStateHandler stateHandler))
            {
                stateHandler.SetDead(false);

                //Debug.Log("Setting state handler death to false");
                //stateHandler.StartCoroutine(stateHandler.RunStateMachine());
            }

        }
    }

    public void Resurrect()
    {
        // Online, the host resurrects the minion and every machine sees it come back (see NetworkEnemy).
        if (NetworkSession.IsOnline && !NetworkSession.IsAuthority && TryGetComponent(out NetworkEnemy networkEnemy) && networkEnemy.IsSpawned)
        {
            networkEnemy.RequestResurrect();
            return;
        }

        //Debug.Log($"Resurrecting minion {Name}");
        ToggleDeath(false);
        SetDead(false);
        ChangeAttribute(AttributeType.CurrentHitpoints, 50f);
        if (ragdollScript != null) ragdollScript.ReverseDestruction();
    }

    /// <summary>Client side: shows a resurrection the host made. The host also sends the new hitpoints.</summary>
    public void ApplyReplicatedResurrection()
    {
        ToggleDeath(false);
        SetDead(false);
        if (ragdollScript != null) ragdollScript.ReverseDestruction();
    }

}


public enum MinionCommands
{
    None,
    GoTo,
    FocusTarget,
    Interact,
}
