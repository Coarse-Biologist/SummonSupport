using UnityEngine;

public class Renew : Ability
{
    public Renew()
    {

        name = "Renew";
        target = AbilityTarget.Summon;
        type = AbilityType.Heal;
        value = 10;
    }
}