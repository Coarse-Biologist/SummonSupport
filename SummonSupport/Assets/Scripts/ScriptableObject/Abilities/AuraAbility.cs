using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Abilities/Aura Ability")]
public class AuraAbility : Ability
{
    [field: SerializeField, Header("Aura settings"), Tooltip("time in [seconds]")]
    public float Radius { get; protected set; } = 2f;

    [field: SerializeField] public GameObject AuraObject { get; protected set; }
    [field: SerializeField] public bool LeaveRotation { get; protected set; } = true;

    public override bool Activate(LivingBeing casterStats)
    {
        int targetNum = 1;

        if (casterStats.CharacterTag != CharacterTag.Enemy)
        {
            targetNum += AbilityModHandler.Instance.GetModAttributeByType(this, AbilityModTypes.Number);
        }

        TeamType desiredTarget = this.GetTargetPreference(casterStats);
        GameObject auraInstance = Instantiate(AuraObject, casterStats.transform.position, AuraObject.transform.rotation, casterStats.transform);
        Aura auraMonoScript = auraInstance.GetComponent<Aura>();
        if (auraMonoScript != null) auraMonoScript.HandleInstantiation(casterStats, null, this);

        List<LivingBeing> targets = GetTargetfromSphereCast(casterStats, casterStats.GetComponent<AbilityHandler>().abilitySpawn.transform, targetNum, desiredTarget);

        foreach (LivingBeing target in targets)
        {
            auraInstance = Instantiate(AuraObject, casterStats.transform.position, AuraObject.transform.rotation, casterStats.transform);
            auraMonoScript = auraInstance.GetComponent<Aura>();
            if (auraMonoScript != null) auraMonoScript.HandleInstantiation(casterStats, null, this);
        }
        return true;
    }
}