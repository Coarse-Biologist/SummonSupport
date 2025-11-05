using System;
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
        Debug.Log($"Activating the targetMouse ability {this.Name}");
        LivingBeing casterStats = user.GetComponent<LivingBeing>();
        Vector2 mousePos;
        if (casterStats.CharacterTag == CharacterTag.Player) mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        else mousePos = user.GetComponent<AIStateHandler>().target.transform.position;
        int layerMask = ~LayerMask.GetMask("Obstruction"); // Alle außer "Obstruction"
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, layerMask);
        bool usedAbility = false;
        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<LivingBeing>(out var target))
            {
                Debug.Log($"target was a living being : {target}");
                
                usedAbility = ActivateAbility(user, target, mousePos);

                Debug.Log($"success in activating ability = {usedAbility}");
                

            }

                if (usedAbility)
                {
                    SpawnEffect(target, user);
                    if (casterStats == target)
                    {
                        CombatStatHandler.HandleEffectPackage(this, casterStats, target, SelfEffects);
                    }
                    else
                    {
                        CombatStatHandler.HandleEffectPackage(this, casterStats, target, TargetEffects);
                    }
                }
        }
        return usedAbility;
    }


    public bool ActivateAbility(GameObject user, LivingBeing targetLivingBeing, Vector2 mousePos)
    {
        if (user.TryGetComponent<AI_CC_State>(out AI_CC_State ccState) && ccState.isCharmed) 
        {
            Debug.Log($"returning false");
            return true;
        }

        if (user.TryGetComponent<LivingBeing>(out var userLivingBeing))
        {
            if (!IsUsableOn(userLivingBeing.CharacterTag, targetLivingBeing.CharacterTag))
            {
                Debug.Log($"returning false");
                return false;
            }
            else 
            {                
                Debug.Log($"returning true");
                return true;
            }
        }
        else 
        {
            Debug.Log($"returning false");
            return false;
        }
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
    



