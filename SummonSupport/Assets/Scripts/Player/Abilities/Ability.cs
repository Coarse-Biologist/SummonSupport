using UnityEngine;

public class Ability
{
    public string name { get; protected set; }
    public AbilityTarget target { get; protected set; }
    public AbilityType type { get; protected set; }
    public int value { get; protected set; }
}