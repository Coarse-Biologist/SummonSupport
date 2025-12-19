using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;




public abstract class Ability : ScriptableObject
{
    [field: SerializeField] public string Name { get; protected set; }
    [field: SerializeField] public Sprite Icon { get; protected set; }
    [field: SerializeField, Range(0, 50)] public float Cooldown { get; protected set; }
    [field: SerializeField] public AttributeType CostType { get; protected set; }
    [field: SerializeField, Min(0)] public float Cost { get; protected set; }
    [field: SerializeField, Min(0)] public float Range { get; protected set; } = 20;
    [field: SerializeField, Min(0)] public float Duration { get; protected set; }
    [field: SerializeField] public AbilityTypeTag AbilityTypeTag { get; protected set; } = AbilityTypeTag.BuffsTarget;
    [field: SerializeField] public List<RelationshipType> ListUsableOn { get; protected set; }
    [field: SerializeField] public EffectPackage TargetEffects { get; protected set; } = new();
    [field: SerializeField] public EffectPackage SelfEffects { get; protected set; } = new();
    [field: SerializeField] public List<Element> ElementTypes { get; protected set; } = new();
    [field: SerializeField] public PhysicalType PhysicalType { get; protected set; } = new();
    [field: SerializeField] public GameObject OnHitEffect { get; protected set; }
    [field: SerializeField] public bool AlterParticleSystemGradient { get; protected set; } = false;




    public abstract bool Activate(GameObject Caster);

    public bool IsUsableOn(CharacterTag user, CharacterTag target)
    {
        RelationshipType relationship = RelationshipHandler.GetRelationshipType(user, target);
        return ListUsableOn.Contains(relationship);
    }
    public bool ThoroughIsUsableOn(LivingBeing caster, LivingBeing target)
    {
        if (caster.HasStatusEffect(StatusEffectType.Madness)) return true;
        return ListUsableOn.Contains(CrewsRelationshipHandler.GetRelationshipType(caster, target));
    }

    public static bool HasElementalSynergy(Ability ability, LivingBeing potentialTarget)
    {
        foreach (Element element in ability.ElementTypes)
        {
            if (potentialTarget.Affinities[element].Get() > 100)
                return true;
        }
        return false;
    }

    public static int GetCoreCraftingCost(Ability ability)
    {
        if (ability == null) throw new System.Exception("You are trying to access the core cost of an ability which is null.");
        return (int)ability.Cooldown * (int)ability.Cost;
    }

    public List<LivingBeing> GetTargetfromSphereCast(Transform directionTransform, int desiredTargetNum, TeamType teamType)
    {
        List<CharacterTag> targetTypes = CrewsRelationshipHandler.TargetTypeToCharTab(teamType);

        List<LivingBeing> targets = new();

        RaycastHit[] hits = Physics.SphereCastAll(directionTransform.position, Range, directionTransform.transform.forward, Range);
        hits = ArrangeByDistanceFromCenter(hits, directionTransform, desiredTargetNum);
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.TryGetComponent(out LivingBeing hitStats)) continue;
            if (targetTypes.Contains(hitStats.CharacterTag))
            {
                targets.Add(hitStats);
            }
            if (targets.Count >= desiredTargetNum) break;
        }
        return targets;
    }

    public RaycastHit[] ArrangeByDistanceFromCenter(RaycastHit[] hits, Transform lookRotation, int desiredTargetNum = 1)
    {
        if (hits == null || hits.Count() <= desiredTargetNum) return hits;
        return hits.OrderBy(g => Vector3.Angle(lookRotation.forward, g.point)).ToArray();
    }

    public TeamType GetTargetPreference(LivingBeing caster)
    {
        if (caster.HasStatusEffect(StatusEffectType.Madness)) return TeamType.Either; // if mad, simply use ability on whoever

        if (AbilityTypeTag == AbilityTypeTag.DebuffsTarget) // if ability is an offensive ability
        {
            if (caster.CharacterTag == CharacterTag.Enemy)
            {
                if (!caster.HasStatusEffect(StatusEffectType.Charmed)) return TeamType.Ally; // UNcharmed enemies will attack enemies
                else return TeamType.Enemy; // otherwise enemies attack Enemies
            }
            else
            {
                if (!caster.HasStatusEffect(StatusEffectType.Charmed)) return TeamType.Enemy; // UNcharmed allies will attack enemies
                else return TeamType.Ally; // otherwise, being insane, allies will attack allies
            }
        }
        else if (AbilityTypeTag == AbilityTypeTag.BuffsTarget) // if ability is an offensive ability
        {
            if (caster.CharacterTag == CharacterTag.Enemy)
            {
                if (!caster.HasStatusEffect(StatusEffectType.Charmed)) return TeamType.Enemy; // UNcharmed enemies will buff enemies
                else return TeamType.Ally; // otherwise theyll buff enemies
            }
            else // if not an enemy
            {
                if (!caster.HasStatusEffect(StatusEffectType.Charmed)) return TeamType.Ally; // UNcharmed allies will buff allies
                else return TeamType.Enemy; // otherwise, being insane, allies will buff enemies
            }
        }
        else throw new System.Exception("Hmmm, under what circumstance should this happen?");
    }

    public void KnockInTheAir(LivingBeing caster, Rigidbody target)
    {
        if (target != null)
        {
            Vector3 pos = caster.transform.position;
            target.AddForce((target.transform.position - new Vector3(pos.x, pos.y - 10, pos.z)).normalized * 30, ForceMode.Impulse);
        }
    }


}


