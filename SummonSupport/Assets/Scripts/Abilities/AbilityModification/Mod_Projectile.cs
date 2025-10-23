using System.Collections.Generic;
using UnityEngine;

public class Mod_Projectile : Mod_Base
{
    public float Speed_Mod { get; protected set; } = 0;
    public float MaxRange_Mod { get; protected set; } = 0;
    public float Lifetime_Mod { get; protected set; } = 0;
    public List<OnHitBehaviour> HitBehaviour_Mod { get; protected set; } = new();
    public int MaxPierce_Mod { get; protected set; } = 0;
    public int MaxSplit_Mod { get; protected set; } = 0;
    public int MaxRicochet_Mod { get; protected set; } = 0;
    public int GetHitAttributeValue(AbilityModTypes modType)
    {
        if (modType == AbilityModTypes.MaxPierce) return MaxPierce_Mod;
        else if (modType == AbilityModTypes.MaxRicochet) return MaxRicochet_Mod;
        else if (modType == AbilityModTypes.MaxSplit) return MaxSplit_Mod;
        else throw new System.Exception($"What attribute do you need a return implimentation for? {modType}");
    }
    public void Mod_OnHitBehaviour(OnHitBehaviour behavior)
    {
        if (!HitBehaviour_Mod.Contains(behavior))
            HitBehaviour_Mod.Add(behavior);
        else throw new System.Exception($"Youre trying to add an on hit behavior {behavior} twice.");
    }
    public void Mod_Pierce(int changeValue)
    {
        Debug.Log("AM I ADDING ANY VALUE TO THE PIERCE MAX?");

        MaxPierce_Mod += changeValue;
    }
    public void Mod_Ricochet(int changeValue)
    {
        Debug.Log("AM I ADDING ANY VALUE TO THE rICOCHET MAX?");

        MaxRicochet_Mod += changeValue;
    }
    public void Mod_Split(int changeValue)
    {
        Debug.Log("AM I ADDING ANY VALUE TO THE SPLIT MAX?");
        MaxSplit_Mod += changeValue;
    }
}
