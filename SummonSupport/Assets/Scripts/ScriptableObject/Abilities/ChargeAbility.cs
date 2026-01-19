using UnityEngine;
[CreateAssetMenu(menuName = "Abilities/Charge Ability")]

public class ChargeAbility : Ability
{
    [field: SerializeField] public Ability ActivateOnHit { private set; get; }
    //[field: SerializeField] public float range { private set; get; }
    [field: SerializeField] public GameObject chargeMonoObject { private set; get; } // object which will handleMonoBehaviors for the charge
    [field: SerializeField] public GameObject chargeTrail { private set; get; }



    public override bool Activate(LivingBeing casterStats)
    {
        Debug.Log("Charge ability is being activated");
        GameObject instance = Instantiate(chargeMonoObject, casterStats.transform.position, Quaternion.identity, casterStats.transform);
        instance.GetComponent<ChargeAbilityMono>().Charge(casterStats, this);
        return true;
    }
}

