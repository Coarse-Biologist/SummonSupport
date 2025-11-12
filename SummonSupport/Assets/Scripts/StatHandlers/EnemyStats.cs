using UnityEngine;
using System.Collections.Generic;
using SummonSupportEvents;


public class EnemyStats : LivingBeing
{
    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability
    [SerializeField] public List<AlchemyLoot> Loot { private set; get; } = new List<AlchemyLoot>();

    public override void Die()
    {
        EventDeclarer.EnemyDefeated.Invoke(this);
        if (HasStatusEffect(StatusEffectType.ExplodeOnDeath)) ViciousDeathExplosion();
        Destroy(gameObject);
    }


}

