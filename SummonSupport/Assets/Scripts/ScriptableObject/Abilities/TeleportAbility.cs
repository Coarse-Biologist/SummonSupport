using System.Collections.Generic;
using UnityEngine;
using System.Collections;


[CreateAssetMenu(menuName = "Abilities/Teleport Ability")]
public class TeleportAbility : Ability
{
    [Header("Teleport settings")]

    [field: SerializeField] public List<Ability> ActivateOnUse { get; private set; }
    [field: SerializeField] public List<Ability> ActivateOnArrive { get; private set; }
    [field: SerializeField] public GameObject EffectOnActivate { get; private set; }
    [field: SerializeField] public float ActivationSpeed { get; private set; } = .3f;







    public bool Activate(GameObject user, Vector2 targetLocation)
    {
        //Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(targetLocation, Vector2.zero);
        GameObject target = null;
        if (hit.collider != null && hit.collider.gameObject.GetComponent<LivingBeing>() != null)
        {
            target = hit.collider.gameObject;

            //Logging.Info($"there was a hit collider");
            if (IsUsableOn(user.GetComponent<LivingBeing>().CharacterTag, target.GetComponent<LivingBeing>().CharacterTag))
            {
                CoroutineManager.Instance.StartCustomCoroutine(TeleportToBeing(user, target));

                return true;
            }
            // else Logging.Info($"the ability wasnt useable on sucha  being");

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
        Instantiate(EffectOnActivate, user.transform.position, Quaternion.identity, user.transform);

        yield return new WaitForSeconds(ActivationSpeed);

        user.transform.position = target.transform.position;

        foreach (Ability ability in ActivateOnArrive)
        {
            ability.Activate(user);
        }

        // handle effects and status effects in package
        //foreach (Crew_EffectPackage statusEffect in StatusEffects)
        //{
        //    //apply status effects
        //}
    }
}
