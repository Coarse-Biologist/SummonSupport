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







    public bool Activate(GameObject user, Vector2 targetLocation)
    {
        RaycastHit[] hits = Physics.SphereCastAll(user.transform.position, Range, user.transform.forward, Range);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.TryGetComponent(out LivingBeing targetStats))
            {
                if (IsUsableOn(user.GetComponent<LivingBeing>().CharacterTag, targetStats.CharacterTag))
                {
                    CoroutineManager.Instance.StartCustomCoroutine(TeleportToBeing(user, targetStats));

                    return true;
                }
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

            user.transform.position = target.transform.position;

            if (ActivateOnArrive != null) ActivateOnArrive.Activate(user);

        }

        else yield return new WaitForSeconds(ActivationSpeed);

    }
}
