using System.Collections.Generic;
using UnityEngine;
using System.Collections;


[CreateAssetMenu(menuName = "Abilities/Teleport Ability")]
public class TeleportAbility : Ability
{
    [Header("Teleport settings")]

    [field: SerializeField] public Ability ActivateOnUse { get; private set; }
    [field: SerializeField] public Ability ActivateOnArrive { get; private set; }
    [field: SerializeField] public GameObject EffectOnActivate { get; private set; }
    [field: SerializeField] public float ActivationSpeed { get; private set; } = .3f;

    private const float BaseCastRadius = 4f;
    private const float SpeedDelayReductionPerMod = .05f;

    public bool Activate(LivingBeing casterStats, Vector3 targetLocation)
    {
        TeamType desiredTargetType = GetTargetPreference(casterStats);

        int desiredTargetNum = 1;
        float castRadius = BaseCastRadius;
        if (casterStats != null && casterStats.CharacterTag != CharacterTag.Enemy && AbilityModHandler.Instance != null)
        {
            desiredTargetNum += AbilityModHandler.Instance.GetModAttributeByType(this, AbilityModTypes.Number);
            castRadius += AbilityModHandler.Instance.GetModAttributeByType(this, AbilityModTypes.Size) * .5f;
        }

        List<LivingBeing> targets = GetTargetfromSphereCast(casterStats, casterStats.abilityHandler.abilitySpawn.transform, desiredTargetNum, desiredTargetType, castRadius);
        List<LivingBeing> validTargets = new();
        foreach (LivingBeing target in targets)
        {
            if (ThoroughIsUsableOn(casterStats, target)) validTargets.Add(target);
        }
        if (validTargets.Count == 0) return false;

        if (ActivateOnUse != null) ActivateOnUse.Activate(casterStats);
        CoroutineManager.Instance.StartCustomCoroutine(TeleportToTargets(casterStats, validTargets));
        return true;
    }

    public override bool Activate(LivingBeing user)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator TeleportToBeing(LivingBeing casterStats, LivingBeing target)
    {
        List<LivingBeing> targets = new() { target };
        yield return TeleportToTargets(casterStats, targets);
    }

    private IEnumerator TeleportToTargets(LivingBeing casterStats, List<LivingBeing> targets)
    {
        float activationDelay = GetActivationDelay(casterStats);

        foreach (LivingBeing target in targets)
        {
            if (casterStats == null || target == null) continue;

            if (EffectOnActivate != null)
                Instantiate(EffectOnActivate, casterStats.transform.position, Quaternion.identity, casterStats.transform);

            yield return new WaitForSeconds(activationDelay);

            casterStats.transform.position = Vector3.Lerp(casterStats.transform.position, target.transform.position, .8f);
            ApplyArrivalEffects(casterStats, target);
        }
    }

    private float GetActivationDelay(LivingBeing casterStats)
    {
        float delay = ActivationSpeed;
        if (casterStats != null && casterStats.CharacterTag != CharacterTag.Enemy && AbilityModHandler.Instance != null)
        {
            int speedMod = AbilityModHandler.Instance.GetModAttributeByType(this, AbilityModTypes.Speed);
            delay -= speedMod * SpeedDelayReductionPerMod;
        }
        return Mathf.Max(.05f, delay);
    }

    private void ApplyArrivalEffects(LivingBeing casterStats, LivingBeing target)
    {
        // Child abilities own their own effect packages, so do not apply this ability's effects twice.
        if (ActivateOnArrive != null)
        {
            ActivateOnArrive.Activate(casterStats);
            return;
        }

        CombatStatHandler.HandleEffectPackage(this, casterStats, target, TargetEffects);
        CombatStatHandler.HandleEffectPackage(this, casterStats, casterStats, SelfEffects);
    }
}
