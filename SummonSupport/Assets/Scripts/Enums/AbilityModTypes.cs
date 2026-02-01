using UnityEngine;

public enum AbilityModTypes
{
    None,
    Cost, // Implimented
    Cooldown, // implimented
    Heal, //implemented
    HealOverTime,//implemented
    Damage, // implimented
    DamageOverTime, // implimented
    Duration, // implimented, universally for DOTs and HOTs, not so for general durations, i.e for conjured abilities
    Size,
    Speed,
    MaxPierce, //  implimented
    MaxRicochet, // rather implimented
    MaxSplit, // implimented
    Number,
    StatusEffect,

}
