using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities.UniversalDelegates;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityModHandler : MonoBehaviour
{
    public Dictionary<Ability, Mod_Base> ModdedAbilities { private set; get; } = new();
    public static readonly Dictionary<AbilityModTypes, int> ModIncrements = new()
    {
        { AbilityModTypes.Cost, -1 },
        { AbilityModTypes.Cooldown, -1 },
        { AbilityModTypes.Damage, 10 },
        { AbilityModTypes.DamageOverTime, 1 },
        { AbilityModTypes.Heal, 5 },
        { AbilityModTypes.HealOverTime, 1 },
        { AbilityModTypes.MakePierce, 1 },
        { AbilityModTypes.MaxPierce, 1 },
        { AbilityModTypes.MakeSplit, 1 },
        { AbilityModTypes.MaxSplit, 1 },
        { AbilityModTypes.MakeRicochet, 1 },
        { AbilityModTypes.MaxRicochet, 1 },
        { AbilityModTypes.Duration, 1 },
        { AbilityModTypes.Range, 1 }

    };
    public static readonly Dictionary<AbilityModTypes, int> ModCosts = new()
    {
        { AbilityModTypes.Cost, 10 },
        { AbilityModTypes.Cooldown, 100 },
        { AbilityModTypes.Damage, 50 },
        { AbilityModTypes.DamageOverTime, 40 },
        { AbilityModTypes.Heal, 60 },
        { AbilityModTypes.HealOverTime, 50 },
        { AbilityModTypes.MakePierce, 300 },
        { AbilityModTypes.MaxPierce, 150 },
        { AbilityModTypes.MakeSplit, 300 },
        { AbilityModTypes.MaxSplit, 150 },
        { AbilityModTypes.MakeRicochet, 300 },
        { AbilityModTypes.MaxRicochet, 150 },
        { AbilityModTypes.Duration, 80 },
        { AbilityModTypes.Range, 30 }

    };
    public static readonly Dictionary<Type, List<AbilityModTypes>> ModOptions = new()
        {
         {typeof(ProjectileAbility), new List<AbilityModTypes>() { AbilityModTypes.MakePierce, AbilityModTypes.MaxPierce, AbilityModTypes.MakeSplit, AbilityModTypes.MaxSplit, AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
             {typeof(TargetMouseAbility),new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
             {typeof(ConjureAbility), new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
            {typeof(AuraAbility), new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
             {typeof(TeleportAbility), new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
             {typeof(MeleeAbility), new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
             {typeof(BeamAbility), new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
             {typeof(ChargeAbility), new List<AbilityModTypes>() { AbilityModTypes.Range, AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime }},
        };

    public static int GetModCost(AbilityModTypes modType)
    {
        if (ModCosts.TryGetValue(modType, out int cost))
        {
            return cost;
        }
        else throw new System.Exception($"The mod whose price you are searching {modType} is not in the Ability Mod cost dictionary!");
    }
    public Mod_Base GetAbilityMod(Ability ability)
    {
        if (ModdedAbilities.TryGetValue(ability, out Mod_Base mod))
            return mod;
        else return null;
    }


    public Mod_Base TryAddNewAbilityMod(Ability ability)
    {
        Mod_Base mod;
        if (!ModdedAbilities.TryGetValue(ability, out Mod_Base existingMod))
        {
            if (ability is ProjectileAbility)
            {
                Debug.Log($"Adding projectile mod for the ability {ability.Name}");
                mod = new Mod_Projectile();
            }
            else
            {
                Debug.Log($"Adding normal mod for the ability {ability.Name}");
                mod = new Mod_Base();
            }
            ModdedAbilities.Add(ability, mod);
            return mod;
        }
        else return existingMod;
    }
    #region Specific mods
    public void Mod_InstantDamage(Ability ability, float modValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.TargetEffects_Mod.Damage.Mod_DamageValue(modValue);
    }
    public void Mod_DotDamage(Ability ability, float modValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.TargetEffects_Mod.DamageOverTime.Mod_DamageValue(modValue);
    }
    public void Mod_HealOverTime(Ability ability, float modValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.TargetEffects_Mod.HealOverTime.Mod_HealValue(modValue);
    }
    public void Mod_Heal(Ability ability, float modValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.TargetEffects_Mod.Heal.Mod_HealValue(modValue);
    }

    public void Mod_Cost(Ability ability, float modValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.Mod_Cost(modValue);
    }
    public void Mod_Cooldown(Ability ability, float modValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.Mod_Cooldown(modValue);
    }
    public void Mod_ModOnHitBehavior(Ability ability, AbilityModTypes modType)
    {
        if (ability is ProjectileAbility)
        {
            Mod_Projectile mod = (Mod_Projectile)TryAddNewAbilityMod(ability);
            switch (modType)
            {
                case AbilityModTypes.MakePierce:
                    {
                        mod.Mod_OnHitBehaviour(OnHitBehaviour.Pierce);
                        mod.Mod_AddAquiredBoolMod(AbilityModTypes.MakePierce);

                        break;
                    }

                case AbilityModTypes.MakeSplit:
                    {
                        mod.Mod_OnHitBehaviour(OnHitBehaviour.Split);
                        mod.Mod_AddAquiredBoolMod(AbilityModTypes.MakeSplit);
                        break;
                    }
                case AbilityModTypes.MakeRicochet:
                    {
                        mod.Mod_OnHitBehaviour(OnHitBehaviour.Ricochet);
                        mod.Mod_AddAquiredBoolMod(AbilityModTypes.MakeRicochet);

                        break;
                    }
                default:
                    break;
            }
        }
    }
    private void Mod_HitBehaviourValue(Ability ability, AbilityModTypes modType)
    {
        Mod_Projectile mod = (Mod_Projectile)TryAddNewAbilityMod(ability);
        switch (modType)
        {
            case AbilityModTypes.MaxRicochet:
                {
                    mod.Mod_Ricochet(1);
                    break;
                }
            case AbilityModTypes.MaxPierce:
                {
                    mod.Mod_Pierce(1);
                    break;
                }
            case AbilityModTypes.MaxSplit:
                {
                    mod.Mod_Split(1);
                    break;
                }
        }
    }
    #endregion
    public void ModAttributeByType(Ability ability, AbilityModTypes modType, float changeValue)
    {
        switch (modType)
        {
            case AbilityModTypes.Damage:
                {
                    Mod_InstantDamage(ability, changeValue);
                    break;
                }
            case AbilityModTypes.DamageOverTime:
                {
                    Mod_DotDamage(ability, changeValue);
                    break;
                }
            case AbilityModTypes.Heal:
                {
                    Mod_Heal(ability, changeValue);
                    break;
                }
            case AbilityModTypes.HealOverTime:
                {
                    Mod_HealOverTime(ability, changeValue);
                    break;
                }
            case AbilityModTypes.Cost:
                {
                    Mod_Cost(ability, changeValue);
                    break;
                }
            case AbilityModTypes.Cooldown:
                {
                    Mod_Cooldown(ability, changeValue);
                    break;
                }

            case AbilityModTypes.MakePierce:
                {
                    Mod_ModOnHitBehavior(ability, modType);

                    break;
                }
            case AbilityModTypes.MakeSplit:
                {
                    Mod_ModOnHitBehavior(ability, modType);
                    break;
                }
            case AbilityModTypes.MakeRicochet:
                {
                    Mod_ModOnHitBehavior(ability, modType);
                    break;
                }
            case AbilityModTypes.MaxRicochet:
                {
                    Mod_HitBehaviourValue(ability, modType);
                    break;
                }
            case AbilityModTypes.MaxPierce:
                {
                    Mod_HitBehaviourValue(ability, modType);
                    break;
                }
            case AbilityModTypes.MaxSplit:
                {
                    Mod_HitBehaviourValue(ability, modType);
                    break;
                }
            default:
                Debug.LogWarning($"no implimentation for {modType}");
                break;
        }

    }

    public int GetModAttributeByType(Ability ability, AbilityModTypes modType)
    {
        if (!ModdedAbilities.TryGetValue(ability, out Mod_Base mod)) return 0;
        else if (modType == AbilityModTypes.Damage)
        {
            return (int)mod.TargetEffects_Mod.Damage.Value;
        }
        else if (modType == AbilityModTypes.Cost)
        {
            return (int)mod.Cost_Mod;
        }
        else if (modType == AbilityModTypes.Cooldown)
        {
            return (int)mod.Cooldown_Mod;
        }
        else if (mod is Mod_Projectile projectileMod)
        {
            Debug.Log($"{modType} found by func = {projectileMod.GetHitAttributeValue(modType)}");
            return projectileMod.GetHitAttributeValue(modType);
        }
        else return 0;
    }
    public List<OnHitBehaviour> GetHitBehaviour(Ability ability)
    {
        if (!ModdedAbilities.TryGetValue(ability, out Mod_Base mod) || mod is not Mod_Projectile projectileMod) return new List<OnHitBehaviour> { };
        else return projectileMod.HitBehaviour_Mod;

    }

    public static int GetModIncrementValue(AbilityModTypes modType)
    {
        if (ModIncrements.TryGetValue(modType, out int num))
        {
            return num;
        }
        else return 0;
    }


    public List<AbilityModTypes> GetModableAttributes(Ability ability)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);

        if (ModOptions.TryGetValue(ability.GetType(), out List<AbilityModTypes> modOptions))
            return GetUncommon(modOptions, mod.AquiredBool_Mods);
        else return new List<AbilityModTypes>();

    }
    public static List<T> GetUncommon<T>(List<T> listA, List<T> listB)
    {
        return listA.Except(listB).Union(listB.Except(listA)).ToList();
    }
    public static string GetAbilityModString(AbilityModTypes modEnum)
    {
        return System.Text.RegularExpressions.Regex.Replace(modEnum.ToString(), "(?<!^)([A-Z])", " $1");
    }
}
