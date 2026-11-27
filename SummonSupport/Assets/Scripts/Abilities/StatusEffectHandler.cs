using System;
using System.Collections;
using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;
using UnityEngine.Animations;

public class StatusEffectHandler : MonoBehaviour
{
    private LivingBeing livingBeing;
    private AIStateHandler stateHandler;
    public Rigidbody rigidBody { private set; get; }
    private float ai_Speed = 5;
    private NetworkCombatant networkCombatant;
    public Dictionary<StatusEffectType, int> SufferedStatusEffects { get; private set; } = new();

    void Awake()
    {
        InitializeStatusEffectDict();
        networkCombatant = GetComponent<NetworkCombatant>();
    }
    void Start()
    {
        if (gameObject.TryGetComponent(out LivingBeing liv))
        {
            livingBeing = liv;
        }
        if (gameObject.TryGetComponent(out AIStateHandler ai))
        {
            stateHandler = ai;
            ai_Speed = stateHandler.navAgent.speed;
        }
        if (gameObject.TryGetComponent(out Rigidbody rb))
        {
            rigidBody = rb;
        }
    }
    public void HandleStatusEffect(StatusEffects status, bool add)
    {
        //Debug.Log($"Adding {status.EffectType} = {true}");

        if (stateHandler == null) return;
        if (add)
        {
            switch (status.EffectType)
            {
                case StatusEffectType.Chilled:
                    {
                        stateHandler.navAgent.speed -= .2f * ai_Speed;
                        stateHandler.anim.anim.speed -= .2f;
                        if (GetStatusEffectValue(StatusEffectType.Chilled) > 4)
                        {
                            stateHandler.navAgent.speed = 0;
                            stateHandler.anim.anim.speed = 0;
                            EventDeclarer.FrozenSolid?.Invoke(livingBeing);
                        }
                        break;
                    }
                case StatusEffectType.Overheated:
                    {
                        EventDeclarer.Overheating?.Invoke(livingBeing);
                        break;
                    }
                case StatusEffectType.Charmed:
                    {
                        stateHandler.SetTargetMask(StatusEffectType.Charmed);
                        break;
                    }
                case StatusEffectType.KnockBack:
                    {
                        KnockInTheAir();
                        break;
                    }
                case StatusEffectType.Madness:
                    {
                        stateHandler.SetTargetMask(StatusEffectType.Madness);
                        break;
                    }
                case StatusEffectType.Pulled:
                    {
                        if (GetStatusEffectValue(StatusEffectType.Pulled) > 0)
                        {
                            EventDeclarer.GraspingVines?.Invoke(livingBeing);
                        }
                        break;
                    }
                case StatusEffectType.Infected:
                    {
                        if (GetStatusEffectValue(StatusEffectType.Infected) > 4)
                        {
                            EventDeclarer.SpreadVirus?.Invoke(livingBeing);
                        }
                        break;
                    }
                case StatusEffectType.Slipping:
                    {
                        if (GetStatusEffectValue(StatusEffectType.Slipping) > 3)
                        {
                            EventDeclarer.Slipping?.Invoke(livingBeing);
                        }
                        break;
                    }
            }
        }
        if (!add)
        {
            switch (status.EffectType)
            {
                case StatusEffectType.Chilled:
                    {
                        //Debug.Log($"Increasing speed by {.2f * ai_Speed}");
                        stateHandler.anim.anim.speed += .2f;
                        stateHandler.navAgent.speed += .2f * ai_Speed;
                        break;
                    }
                case StatusEffectType.Charmed:
                    {
                        stateHandler.SetTargetMask(StatusEffectType.Charmed);
                        break;
                    }
                case StatusEffectType.Madness:
                    {
                        stateHandler.SetTargetMask(StatusEffectType.Madness);
                        break;
                    }
                case StatusEffectType.Poisoned:
                    stateHandler.SetStateMachineSpeed(GetStatusEffectValue(StatusEffectType.Poisoned));
                    break;


            }
        }
    }
    public void AlterStatusEffectList(StatusEffects status, bool Add) // modifies the list of abilities by which one is affected
    {
        // Online, the host decides the status effects of networked beings and tells every client
        // (see NetworkCombatant.ShareStatusEffect), which applies them with ApplyReplicatedStatusEffect.
        if (networkCombatant != null && networkCombatant.IsSpawned)
        {
            if (!networkCombatant.IsServer) return;
            networkCombatant.ShareStatusEffect(status, Add);
        }

        ChangeStatusEffect(status, Add);
        if (Add) StartCoroutine(RemoveStatusEffect(status, 2 * status.Duration));
    }

    /// <summary>Applies a status effect change the host sent. The host also says when it ends.</summary>
    public void ApplyReplicatedStatusEffect(StatusEffects status, bool add)
    {
        ChangeStatusEffect(status, add);
    }

    private void ChangeStatusEffect(StatusEffects status, bool add)
    {
        if (add)
        {
            if (livingBeing is EnemyStats enemyStats)
            {
                enemyStats.AddStatusEffectSymbol(status, 1);
            }
            SufferedStatusEffects[status.EffectType] += 1;
            HandleStatusEffect(status, true);
        }
        else
        {
            // A client that joined while the effect was on never saw it start, so there is nothing to end.
            if (SufferedStatusEffects[status.EffectType] <= 0) return;
            SufferedStatusEffects[status.EffectType] -= 1;
            HandleStatusEffect(status, false);
        }
    }
    private IEnumerator RemoveStatusEffect(StatusEffects status, float duration)
    {
        yield return new WaitForSeconds(duration);

        AlterStatusEffectList(status, false);
    }
    public bool HasStatusEffect(StatusEffectType status)
    {
        return SufferedStatusEffects[status] > 0;
    }

    public int GetStatusEffectValue(StatusEffectType status)
    {
        int statusValue = SufferedStatusEffects[status];

        return statusValue;// SufferedStatusEffects[status];
    }

    void InitializeStatusEffectDict()
    {
        SufferedStatusEffects = new Dictionary<StatusEffectType, int>();

        foreach (StatusEffectType effect in Enum.GetValues(typeof(StatusEffectType)))
        {
            SufferedStatusEffects[effect] = 0;
        }
    }
    private void KnockInTheAir()
    {
        Debug.Log($"knocking in the air");

        if (rigidBody == null) return;
        rigidBody.AddForce(-transform.forward * GetStatusEffectValue(StatusEffectType.KnockBack) * 20, ForceMode.Impulse);
    }


}
