using UnityEngine;
[CreateAssetMenu(menuName = "Abilities/Charge Ability")]

public class ChargeAbility : Ability
{
    [field: SerializeField] public Ability ability { private set; get; }
    [field: SerializeField] public float range { private set; get; }
    [field: SerializeField] public GameObject chargeMonoObject { private set; get; } // object which will handleMonoBehaviors for the charge

    public override bool Activate(GameObject Caster)
    {
        Debug.Log("Charge ability is being activated");
        GameObject instance = Instantiate(chargeMonoObject, Caster.transform.position, Quaternion.identity, Caster.transform);
        instance.GetComponent<ChargeAbilityMono>().Charge(this);
        return true;
    }
}

