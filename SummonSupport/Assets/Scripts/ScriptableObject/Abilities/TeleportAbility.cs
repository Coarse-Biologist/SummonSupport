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







    public bool Activate(GameObject user, Vector3 targetLocation)
    {
        LivingBeing casterStats = user.GetComponent<LivingBeing>();
        //RaycastHit[] hits = Physics.SphereCastAll(user.transform.position, Range, user.transform.forward, Range);
        TeamType desiredTargetType = this.GetTargetPreference(casterStats);

        List<LivingBeing> targets = GetTargetfromSphereCast(user.GetComponent<AbilityHandler>().abilitySpawn.transform, 1, desiredTargetType);

        foreach (LivingBeing target in targets)
        {

            if (IsUsableOn(casterStats.CharacterTag, target.CharacterTag))
            {
                CoroutineManager.Instance.StartCustomCoroutine(TeleportToBeing(user, target));

                return true;
            }

        }
        return false;
    }

    public override bool Activate(GameObject user)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator TeleportToBeing(GameObject user, LivingBeing target)
    {
        if (target != null && user != null)
        {
            Instantiate(EffectOnActivate, user.transform.position, Quaternion.identity, user.transform);

            yield return new WaitForSeconds(ActivationSpeed);

            user.transform.position = target.transform.position + (user.transform.position * .1f);

            if (ActivateOnArrive != null) ActivateOnArrive.Activate(user);

        }

        else yield return new WaitForSeconds(ActivationSpeed);

    }
}
