using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Aura: MonoBehaviour
{
    LivingBeing caster;
    Ability ability;
    List<LivingBeing> listLivingBeingsInAura = new();

    public void HandleInstanciation(LivingBeing caster, Ability ability)
    {
        this.caster = caster;
        this.ability = ability;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.TryGetComponent(out LivingBeing otherLivingBeing))
            return;

        RelationshipType relationship = RelationshipHandler.GetRelationshipType(caster.CharacterTag, otherLivingBeing.CharacterTag);
        if (ability.ListUsableOn.Contains(relationship) && !listLivingBeingsInAura.Contains(otherLivingBeing))
            AddAuraEffectToLivingBeing(otherLivingBeing);
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
