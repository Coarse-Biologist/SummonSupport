using UnityEngine;
using System.Collections.Generic;

public class EnemyStats : LivingBeing
{
    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability
    [SerializeField] public List<AlchemyLoot> Loot { private set; get; } = new List<AlchemyLoot>();

    public MinionCommands CurrentCommand { get; private set; } = MinionCommands.None;


    void Start()
    {

        healthbarInterface = GetComponent<I_HealthBar>();

        if (healthbarInterface != null)
        {
            healthbarInterface.SetHealthBarValue(GetAttribute(AttributeType.CurrentHitpoints));
            healthbarInterface.SetHealthBarMaxValue(GetAttribute(AttributeType.MaxHitpoints));
        }


    }


}

