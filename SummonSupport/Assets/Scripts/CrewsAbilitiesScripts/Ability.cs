using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;



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





    public abstract bool Activate(GameObject Caster);

    public bool IsUsableOn(CharacterTag user, CharacterTag target)
    {
        RelationshipType relationship = RelationshipHandler.GetRelationshipType(user, target);
        return ListUsableOn.Contains(relationship);
    }
    public bool ThoroughIsUsableOn(LivingBeing caster, LivingBeing target)
    {
        if (caster.TryGetComponent<AI_CC_State>(out AI_CC_State ccState) && ccState.isMad) return true;
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

    public List<LivingBeing> GetTargetfromSphereCast(Ability ability, Transform directionTransform, int desiredTargetNum, CharacterTag targetType)
    {
        List<LivingBeing> targets = new();

        RaycastHit[] hits = Physics.SphereCastAll(directionTransform.position, ability.Range, directionTransform.transform.forward, Range);
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.TryGetComponent(out LivingBeing hitStats)) continue;
            if (hitStats.CharacterTag == targetType)
            {
                targets.Add(hitStats);
            }
            if (targets.Count >= desiredTargetNum) break;
        }
        return targets;
    }


}


