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
    [field: SerializeField] public AbilitySoundPackage Sounds { get; protected set; }


    public string DisplayAbilityInfo()
    {
        string abilityInfo = $"{Name}";

        abilityInfo += $"\nAbility type : {GeneralFunctions.GetCleanEnumString(AbilityTypeTag)}";
        abilityInfo += $"\nCost : {Cost} {GeneralFunctions.GetCleanEnumString(CostType)}";
        abilityInfo += $"\nCooldown : {Cooldown} seconds";

        string elementTypes = "";
        if (ElementTypes.Count() > 0)
        {
            foreach (Element element in ElementTypes)
            {
                elementTypes += $"{element}, ";
            }
            abilityInfo += $"Element Types : {elementTypes}";
        }

        if (PhysicalType != PhysicalType.None)
        {
            abilityInfo += $"\nPhysical Type : {PhysicalType}";
        }
        string targetEffects = TargetEffects.GetPackageInfo();
        if (targetEffects != "")
        {
            abilityInfo += $"\n Target Effects : {TargetEffects.GetPackageInfo()}";
        }
        string selfEffects = SelfEffects.GetPackageInfo();
        if (selfEffects != "")
        {
            abilityInfo += $"\n Self Effects : {SelfEffects.GetPackageInfo()}";
        }
        return abilityInfo;
    }

    public abstract bool Activate(LivingBeing Caster);


    public bool ThoroughIsUsableOn(LivingBeing caster, LivingBeing target)
    {
        if (caster.SE_Handler != null && caster.SE_Handler.HasStatusEffect(StatusEffectType.Madness) || caster.SE_Handler.HasStatusEffect(StatusEffectType.Charmed)) return true;
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
        int modifier = 1; //#TODO find better solution
        if (ability is BeamAbility beam) modifier += 10;
        return ((int)ability.Cooldown + 1) * ((int)ability.Cost + 1) * (1 + modifier);
    }

    public List<LivingBeing> GetTargetfromSphereCast(LivingBeing casterStats, Transform directionTransform, int desiredTargetNum, TeamType teamType)
    {
        List<CharacterTag> targetTypes = CrewsRelationshipHandler.TargetTypeToCharTab(teamType);

        List<LivingBeing> targets = new();
        RaycastHit[] hits = Physics.SphereCastAll(directionTransform.position, 4, directionTransform.transform.forward, Range);
        hits = ArrangeByDistanceFromCenter(hits, directionTransform, desiredTargetNum);
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.TryGetComponent(out LivingBeing hitStats) || hitStats == casterStats) continue;
            if (targetTypes.Contains(hitStats.CharacterTag))
            {
                targets.Add(hitStats);
                Debug.Log($"Hit found {hit}");
                SetupManager.Instance.DebugLocation(hitStats.transform.position, Color.blueViolet, 2);
                Debug.DrawLine(
        directionTransform.position,
        hit.point,
        Color.yellow,
        2f
    );
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
    public TeamType GetTargetPreference(PlayerStats playerStats)
    {
        if (AbilityTypeTag == AbilityTypeTag.BuffsTarget)
        {
            return TeamType.Ally;
        }
        else return TeamType.Enemy;
    }

    public TeamType GetTargetPreference(LivingBeing caster)
    {

        if (caster.SE_Handler.HasStatusEffect(StatusEffectType.Madness)) return TeamType.Either; // if mad, simply use ability on whoever

        if (AbilityTypeTag == AbilityTypeTag.DebuffsTarget) // if ability is an offensive ability
        {
            if (caster.CharacterTag == CharacterTag.Enemy)
            {
                if (!caster.SE_Handler.HasStatusEffect(StatusEffectType.Charmed)) return TeamType.Ally; // UNcharmed enemies will attack enemies
                else return TeamType.Enemy; // otherwise enemies attack Enemies
            }
            else
            {
                if (!caster.SE_Handler.HasStatusEffect(StatusEffectType.Charmed)) return TeamType.Enemy; // UNcharmed allies will attack enemies
                else return TeamType.Ally; // otherwise, being insane, allies will attack allies
            }
        }
        else if (AbilityTypeTag == AbilityTypeTag.BuffsTarget) // if ability is an offensive ability
        {
            if (caster.CharacterTag == CharacterTag.Enemy)
            {
                if (!caster.SE_Handler.HasStatusEffect(StatusEffectType.Charmed)) return TeamType.Enemy; // UNcharmed enemies will buff enemies
                else return TeamType.Ally; // otherwise theyll buff enemies
            }
            else // if not an enemy
            {
                if (!caster.SE_Handler.HasStatusEffect(StatusEffectType.Charmed)) return TeamType.Ally; // UNcharmed allies will buff allies
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


