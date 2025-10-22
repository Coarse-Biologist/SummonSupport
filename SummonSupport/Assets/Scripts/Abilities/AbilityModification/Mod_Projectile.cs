using UnityEngine;

public class Mod_Projectile : Mod_Base
{
    public float Speed_Mod { get; protected set; } = 0;
    public float MaxRange_Mod { get; protected set; } = 0;
    public float Lifetime_Mod { get; protected set; } = 0;
    public OnHitBehaviour HitBehaviour_Mod { get; protected set; } = OnHitBehaviour.Destroy;
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
        HitBehaviour_Mod = behavior;
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
