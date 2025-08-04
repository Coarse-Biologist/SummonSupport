using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Target Mouse Ability")]
public class TargetMouseAbility : Ability
{
    [field: Header("settings")]
    [field: SerializeField] public GameObject SpawnEffectOnHit { get; set; }
    [field: SerializeField] public EffectOrientation EffectOrientation { get; set; } = EffectOrientation.Identity;



    public override bool Activate(GameObject user)
    {
        LivingBeing casterStats = user.GetComponent<LivingBeing>();
        bool onSelf = false;
        bool usedAbility = false;
        Vector2 mousePos;
        if (casterStats.CharacterTag == CharacterTag.Player) mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        else mousePos = user.GetComponent<AIStateHandler>().target.transform.position;
        int layerMask = ~LayerMask.GetMask("Obstruction"); // Alle außer "Obstruction"
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, layerMask);
        //Collider2D[] rangeChecks = Physics2D.OverlapCircleAll(mousePos, 1, stateHandler.targetMask);

        if (hit.collider != null)
            if (hit.collider.TryGetComponent<LivingBeing>(out var target))
            {
                if (target == casterStats) onSelf = true;
                usedAbility = ActivateAbility(user, target, mousePos);
                CombatStatHandler.HandleEffectPackages(this, casterStats, target, onSelf);
            }

        return usedAbility;
    }


    public bool ActivateAbility(GameObject user, LivingBeing targetLivingBeing, Vector2 mousePos)
    {
        if (user.TryGetComponent<LivingBeing>(out var userLivingBeing))
            if (!IsUsableOn(userLivingBeing.CharacterTag, targetLivingBeing.CharacterTag))
                return false;

        SpawnEffect(targetLivingBeing, user);

        //cause all effects that should happen on successful hit
        return true;
    }


    private void SpawnEffect(LivingBeing targetLivingBeing, GameObject Caster)
    {
        GameObject instance;
        Quaternion rotation = Quaternion.identity;
        Transform effectChild; // if there is a special force field which must be handled separately
        if (EffectOrientation == EffectOrientation.TowardCaster)
        {
            var direction = (targetLivingBeing.transform.position - Caster.transform.position).normalized;
            rotation = Quaternion.LookRotation(direction);
        }
        if (SpawnEffectOnHit != null)
        {
            instance = Instantiate(SpawnEffectOnHit, targetLivingBeing.transform.position, rotation, targetLivingBeing.transform.transform);
            if (instance.transform.GetComponentInChildren<ParticleSystemForceField>() != null)
            {

                effectChild = instance.transform.GetComponentInChildren<ParticleSystemForceField>().transform;
                effectChild.transform.position = Caster.transform.position;
                effectChild.transform.SetParent(Caster.transform);

                Destroy(effectChild.gameObject, 3f);
            }
            Destroy(instance, 3f);

        }
    }
}

