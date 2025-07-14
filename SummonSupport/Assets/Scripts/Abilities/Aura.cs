using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Aura : MonoBehaviour
{
    LivingBeing caster;
    Ability ability;
    List<LivingBeing> listLivingBeingsInAura = new();
    private bool Active = false;



    public void HandleInstanciation(LivingBeing caster, Ability ability)
    {
        this.caster = caster;
        this.ability = ability;
        Invoke("Activate", ability.ActivationTime);
    }
    public void SetAuraStats(GameObject caster, AuraAbility ability)
    {
        this.caster = caster.GetComponent<LivingBeing>();
        this.ability = ability;
        SetAuraTimer(ability.Uptime);
        //transform.Rotate(new Vector3(-110f, 0, 0));

    }

    private void Activate()
    {
        Active = true;
    }

    public void SetAuraTimer(float timeUp)
    {
        Destroy(gameObject, timeUp);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (Active)
        {
            Logging.Info($"{other.gameObject} has entered the bite zone");
            if (other.gameObject.TryGetComponent(out LivingBeing otherLivingBeing))

                if (ability.ListUsableOn.Contains(RelationshipHandler.GetRelationshipType(caster.CharacterTag, otherLivingBeing.CharacterTag)) && !listLivingBeingsInAura.Contains(otherLivingBeing))
                {
                    AddAuraEffectToLivingBeing(otherLivingBeing);
                    otherLivingBeing.ChangeAttribute(ability.Attribute, ability.Value);
                }
        }
    }
    void AddAuraEffectToLivingBeing(LivingBeing otherLivingBeing)
    {
        foreach (StatusEffect statusEffect in ability.StatusEffects)
        {
            if (otherLivingBeing.activeStatusEffects.ContainsKey(statusEffect.Name))
                continue;
            statusEffect.ApplyStatusEffect(otherLivingBeing);
            listLivingBeingsInAura.Add(otherLivingBeing);
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        LivingBeing otherLivingBeing = other.gameObject.GetComponent<LivingBeing>();
        if (listLivingBeingsInAura.Contains(otherLivingBeing))
            RemoveAuraEffectFromLivingBeing(otherLivingBeing);
    }
    void RemoveAuraEffectFromLivingBeing(LivingBeing otherLivingBeing)
    {
        if (!listLivingBeingsInAura.Remove(otherLivingBeing))
            return;
        foreach (StatusEffect statusEffect in ability.StatusEffects)
            statusEffect.RemoveStatusEffect(otherLivingBeing);

    }
    void OnDestroy()
    {
        foreach (LivingBeing livingBeing in listLivingBeingsInAura.ToList())
            RemoveAuraEffectFromLivingBeing(livingBeing);
        // Use ToList() to create a copy of the list, because 
        // RemoveAuraEffectFromLivingBeing modifies the original list.
    }
}
