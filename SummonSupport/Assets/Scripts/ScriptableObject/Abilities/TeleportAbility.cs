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







    public bool Activate(LivingBeing casterStats, Vector3 targetLocation)
    {
        //RaycastHit[] hits = Physics.SphereCastAll(user.transform.position, Range, user.transform.forward, Range);
        TeamType desiredTargetType = this.GetTargetPreference(casterStats);

        List<LivingBeing> targets = GetTargetfromSphereCast(casterStats, casterStats.abilityHandler.abilitySpawn.transform, 1, desiredTargetType);
        if (ActivateOnUse != null) ActivateOnUse.Activate(casterStats);
        foreach (LivingBeing target in targets)
        {

            if (IsUsableOn(casterStats.CharacterTag, target.CharacterTag))
            {
                CoroutineManager.Instance.StartCustomCoroutine(TeleportToBeing(casterStats, target));

                return true;
            }

        }
        return false;
    }

    public override bool Activate(LivingBeing user)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator TeleportToBeing(LivingBeing casterStats, LivingBeing target)
    {
        if (target != null && casterStats != null)
        {
            Instantiate(EffectOnActivate, casterStats.transform.position, Quaternion.identity, casterStats.transform);

            yield return new WaitForSeconds(ActivationSpeed);


            //casterStats.transform.position = target.transform.position + (casterStats.transform.position * .1f);
            casterStats.transform.position = Vector3.Lerp(casterStats.transform.position, target.transform.position, 0.8f
            );
            if (ActivateOnArrive != null) ActivateOnArrive.Activate(casterStats);

        }

        else yield return new WaitForSeconds(ActivationSpeed);

    }
}
