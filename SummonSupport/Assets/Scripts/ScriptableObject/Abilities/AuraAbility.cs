using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Abilities/Aura Ability")]
public class AuraAbility : Ability
{
    [field: SerializeField, Header("Aura settings"), Tooltip("time in [seconds]")]
    public float Radius { get; protected set; } = 2f;

    [field: SerializeField] public GameObject AuraObject { get; protected set; }
    [field: SerializeField] public bool LeaveRotation { get; protected set; } = true;

    [field: SerializeField, Tooltip("How far a split aura searches for a new target.")]
    public float SplitSearchRadius { get; protected set; } = 10f;

    public override bool Activate(LivingBeing casterStats)
    {
        int targetNum = 1;
        int sizeMod = 0;
        int speedMod = 0;

        if (casterStats != null && casterStats.CharacterTag != CharacterTag.Enemy)
        {
            targetNum += Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Number, casterStats));
            sizeMod = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Size, casterStats));
            speedMod = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Speed, casterStats));
        }

        TeamType desiredTarget = this.GetTargetPreference(casterStats);
        Transform auraGroup = Aura.CreateAuraGroup(casterStats, this);
        Transform auraParent = auraGroup != null ? auraGroup : casterStats.transform;

        GameObject auraInstance = Instantiate(AuraObject, casterStats.transform.position, AuraObject.transform.rotation, auraParent);
        Aura auraMonoScript = auraInstance.GetComponent<Aura>();
        if (auraMonoScript != null) auraMonoScript.HandleInstantiation(casterStats, null, this);

        float castRadius = Radius + sizeMod;
        float castRange = Range + speedMod * .5f;
        List<LivingBeing> targets = GetTargetfromSphereCast(casterStats, casterStats.abilityHandler.abilitySpawn.transform, targetNum, desiredTarget, castRadius, castRange);

        foreach (LivingBeing target in targets)
        {
            auraInstance = Instantiate(AuraObject, casterStats.transform.position, AuraObject.transform.rotation, auraParent);
            auraMonoScript = auraInstance.GetComponent<Aura>();
            if (auraMonoScript != null) auraMonoScript.HandleInstantiation(casterStats, null, this);
        }
        return true;
    }
}