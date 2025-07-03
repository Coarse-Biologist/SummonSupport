using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AbilityUseHandler_1 : MonoBehaviour
{
    [field: SerializeField] public Crew_Ability_SO Ability_SO;

    [field: SerializeField] public Vector2 MouseLoc;
    private static LivingBeing casterStats;
    private static List<CharacterTag> friendlies = new List<CharacterTag> { CharacterTag.Player, CharacterTag.Minion, CharacterTag.Guard };
    public void Awake()
    {
        casterStats = gameObject.GetComponent<LivingBeing>();
    }

    // called in player movement class when player presses ability button
    public void CheckSufficientResource(Crew_Ability_SO Ability, Vector2 Loc)
    {
        Ability_SO = Ability;
        if (casterStats != null && casterStats.GetAttribute(Ability.CostType) > Ability_SO.Cost) // add check for cooldowns
        {
            MouseLoc = Loc;
            IterateAbilityEffects();
        }

    }
    // called in check sufficient resource func if castable
    private void IterateAbilityEffects()
    {
        // iterate through all effects of the ability
        foreach (Crew_EffectPackage effectPackage in Ability_SO.TargetTypeAndEffects)
        {
            //Debug.Log($"{effectPackage.Damage} = damage. {effectPackage.Heal} = heal. {effectPackage.HealOverTime} = heal over time");
            DetermineAbilityUseMethod(effectPackage);
        }
    }
    // called in iterate ability effect func
    private void DetermineAbilityUseMethod(Crew_EffectPackage effectPackage)
    {
        switch (effectPackage.TargetType)
        {

            case Crew_TargetType.Projectile:
                Crew_AbilityObjectSpawner.SpawnProjectile(casterStats, effectPackage, transform.position, MouseLoc);
                //Debug.Log("effect package contained a projectile ability!");

                break;
            case Crew_TargetType.Self:
                if (!effectPackage.HasAoE) HandleEffects(casterStats, casterStats, effectPackage);
                else Crew_AbilityObjectSpawner.SpawnAura(effectPackage, casterStats);

                break;
            case Crew_TargetType.OnTarget:
                //Debug.Log("effect package contained an on target ability!");

                break;
        }
    }

    public static void HandleEffects(LivingBeing casterStats, LivingBeing targetStats, Crew_EffectPackage effectPackage)
    {
        bool isfriendlyRelationship = IsCasterTargetFriendly(casterStats, targetStats);
        if (isfriendlyRelationship)
        {
            if (effectPackage.Heal.Value > 0)
                // handle heal 
                CombatStatHandler.AdjustHealValue(effectPackage.Heal.Value, targetStats, casterStats);

            if (effectPackage.HealOverTime.Value > 0)
                CombatStatHandler.ApplyAttributeRepeatedly(targetStats, AttributeType.CurrentHitpoints, effectPackage.HealOverTime.Value, effectPackage.HealOverTime.Duration);

            foreach (TempAttrIncrease_AT attr_Up in effectPackage.AttributeUp)
                CombatStatHandler.ApplyTempValue(targetStats, attr_Up.AttributeType, attr_Up.Value, attr_Up.Duration);
        }
        else
        {
            foreach (TempAttrDecrease_AT attr_Down in effectPackage.AttributeDown)
                CombatStatHandler.ApplyTempValue(targetStats, attr_Down.AttributeType, -attr_Down.Value, attr_Down.Duration);

            foreach (Damage_AT damage in effectPackage.Damage)
                CombatStatHandler.AdjustDamageValue(damage.Element, damage.Value, targetStats, casterStats);

            foreach (DamageoT_AT DOT in effectPackage.DamageOverTime)
                CombatStatHandler.AdjustAndApplyDOT(DOT.Element, DOT.Value, DOT.Duration, targetStats, casterStats);
        }
    }

    private static bool IsCasterTargetFriendly(LivingBeing CasterStats, LivingBeing targetStats)
    {
        if (friendlies.Contains(CasterStats.CharacterTag) && targetStats.CharacterTag == CharacterTag.Enemy || (casterStats.CharacterTag == CharacterTag.Enemy && friendlies.Contains(targetStats.CharacterTag)))
            return false;
        else return true;

    }




}
