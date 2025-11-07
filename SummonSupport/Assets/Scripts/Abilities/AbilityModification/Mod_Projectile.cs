//using System.Collections.Generic;
//using UnityEngine;
//
//public class Mod_Projectile : Mod_Base
//{
//    public float ProjectileSpeed_Mod { get; protected set; } = 0;
//    public int MaxPierce_Mod { get; protected set; } = 0;
//    public int MaxSplit_Mod { get; protected set; } = 0;
//    public int MaxRicochet_Mod { get; protected set; } = 0;
//
//    public List<OnHitBehaviour> HitBehaviour_Mod { get; protected set; } = new();
//
//
//    public Dictionary<Element, (Func<float> Get, Action<float> Set)> Projectile_Mods = new Dictionary<Element, (Func<float> Get, Action<float> Set)>
//            {
//                {AbilityModTypes.ProjectileSpeed_Mod,    (() => ProjectileSpeed_Mod,    v => ProjectileSpeed_Mod = v) },
//                {AbilityModTypes.MaxPierce_Mod,          (() => MaxPierce_Mod,          v => MaxPierce_Mod = v) },
//                {AbilityModTypes.MaxSplit_Mod,           (() => MaxSplit_Mod,           v => MaxSplit_Mod = v) },
//                {AbilityModTypes.MaxRicochet_Mod,        (() => MaxRicochet_Mod,           v => MaxRicochet_Mod = v) },
//            };
//
//
//
//    public void Mod_ProjectileAttribute(AbilityModTypes modType, float changeValue)
//    {
//        if (Projectile_Mods.TryGetValue(modType, out (Func<float> Get, Action<float> Set) func))
//        {
//            Projectile_Mods[element].Set(changeValue + GetProjectileAttribute(modType));
//        }
//        else throw new System.Exception("You are trying to modify an an attribute which projectiles cannot change");
//    }
//
//    public int GetProjectileAttribute(AbilityModTypes modType)
//    {
//        if (Projectile_Mods.TryGetValue(modType, out (Func<float> Get, Action<float> Set) func))
//        {
//            return (int)Affinities[element].Get();
//        }
//        else
//        {
//            throw new System.Exception("You are trying to modify an attribute which projectiles cannot change");
//            return 0;
//        }
//    }
//
//
//    public void Mod_OnHitBehaviour(OnHitBehaviour behavior)
//    {
//        if (!HitBehaviour_Mod.Contains(behavior))
//            HitBehaviour_Mod.Add(behavior);
//        else throw new System.Exception($"Youre trying to add an on hit behavior {behavior} twice.");
//    }
//    public void Mod_Pierce(int changeValue)
//    {
//
//        MaxPierce_Mod += changeValue;
//    }
//    public void Mod_Ricochet(int changeValue)
//    {
//
//        MaxRicochet_Mod += changeValue;
//    }
//    public void Mod_Split(int changeValue)
//    {
//        MaxSplit_Mod += changeValue;
//    }
//    public void Mod_ProjectileSpeed(int changeValue)
//    {
//        ProjectileSpeed_Mod += changeValue;
//    }
//}
