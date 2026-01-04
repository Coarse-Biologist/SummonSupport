using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Abilities/Aura Ability")]
public class AuraAbility : Ability
{
    [field: SerializeField, Header("Aura settings"), Tooltip("time in [seconds]")]
    public float Radius { get; protected set; } = 2f;

    [field: SerializeField] public GameObject AuraObject { get; protected set; }
    [field: SerializeField] public bool LeaveRotation { get; protected set; } = true;

    public override bool Activate(GameObject caster)
    {
        int targetNum = 1;

        if (caster.TryGetComponent(out AbilityModHandler modHandler))
        {
            targetNum += modHandler.GetModAttributeByType(this, AbilityModTypes.Number);
        }

        LivingBeing casterStats = caster.GetComponent<LivingBeing>();
        TeamType desiredTarget = this.GetTargetPreference(casterStats);
        GameObject auraInstance = Instantiate(AuraObject, caster.transform.position, AuraObject.transform.rotation, caster.transform);
        Aura auraMonoScript = auraInstance.GetComponent<Aura>();
        if (auraMonoScript != null) auraMonoScript.HandleInstantiation(casterStats, null, this);

        List<LivingBeing> targets = GetTargetfromSphereCast(caster.GetComponent<AbilityHandler>().abilitySpawn.transform, targetNum, desiredTarget);

        foreach (LivingBeing target in targets)
        {
            auraInstance = Instantiate(AuraObject, caster.transform.position, AuraObject.transform.rotation, caster.transform);
            auraMonoScript = auraInstance.GetComponent<Aura>();
            if (auraMonoScript != null) auraMonoScript.HandleInstantiation(casterStats, null, this);
        }
        return true;
    }
}