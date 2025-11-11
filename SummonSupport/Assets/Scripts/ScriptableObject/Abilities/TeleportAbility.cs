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
        GameObject target = null;
        RaycastHit[] hits = Physics.SphereCastAll(user.transform.position, Range, user.transform.forward, Range);
        foreach (RaycastHit hit in hits)
        {
            //Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (hit.collider.gameObject.TryGetComponent(out LivingBeing targetStats))
            {
                if (IsUsableOn(user.GetComponent<LivingBeing>().CharacterTag, targetStats.CharacterTag))
                {
                    CoroutineManager.Instance.StartCustomCoroutine(TeleportToBeing(user, target));

                    return true;
                }
            }
        }
        //else Logging.Info($"there was NO hit collider or it wasnt a living being at location {hit.point}");

        return false;
    }

    public override bool Activate(GameObject user)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator TeleportToBeing(GameObject user, GameObject target)
    {
        if (target != null && user != null)
        {
            if (!user.TryGetComponent(out LivingBeing livingBeing)) yield return new WaitForSeconds(ActivationSpeed);
            else
            {
                GameObject onHitEffectInstance = Instantiate(EffectOnActivate, user.transform.position, Quaternion.identity, user.transform);

                EffectColorChanger.ChangeObjectsParticleSystemColor(livingBeing, onHitEffectInstance);

                yield return new WaitForSeconds(ActivationSpeed);

                user.transform.position = target.transform.position;

                ActivateOnArrive.Activate(user);

            }
        }
        else yield return new WaitForSeconds(ActivationSpeed);

    }
}
