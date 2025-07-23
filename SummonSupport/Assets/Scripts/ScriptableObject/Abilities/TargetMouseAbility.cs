using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Target Mouse Ability")]
public class TargetMouseAbility : Ability
{
    [field: Header("settings")]
    [field: SerializeField] public GameObject SpawnEffectOnHit { get; set; }


    public override bool Activate(GameObject user)
    {
        bool usedAbility = false;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int layerMask = ~LayerMask.GetMask("Obstruction"); // Alle außer "Obstruction"
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, layerMask);
        //Collider2D[] rangeChecks = Physics2D.OverlapCircleAll(mousePos, 1, stateHandler.targetMask);

        if (hit.collider != null)
            if (hit.collider.TryGetComponent<LivingBeing>(out var target))
            {
                usedAbility = ActivateAbility(user, target, mousePos);
                SpawnEffect(target);
            }

        return usedAbility;
    }


    public bool ActivateAbility(GameObject user, LivingBeing targetLivingBeing, Vector2 mousePos)
    {
        if (user.TryGetComponent<LivingBeing>(out var userLivingBeing))
            if (!IsUsableOn(userLivingBeing.CharacterTag, targetLivingBeing.CharacterTag))
                return false;

        SpawnEffect(targetLivingBeing);

        //cause all effects that should happen on successful hit
        return true;
    }


    private void SpawnEffect(LivingBeing targetLivingBeing)
    {
        GameObject instance;
        if (SpawnEffectOnHit != null)
        {
            instance = Instantiate(SpawnEffectOnHit, targetLivingBeing.transform.position, Quaternion.identity, targetLivingBeing.transform.transform);
            Destroy(instance, 3f);
        }
    }
}