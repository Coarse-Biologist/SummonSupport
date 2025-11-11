using UnityEngine;
using UnityEngine.UI;

public class ConjuredObject : MonoBehaviour
{
    public ConjureAbility Ability { get; private set; }
    public LivingBeing Conjurer { get; private set; }
    private float radius;


    public void SetAbility(ConjureAbility ability)
    {
        Ability = ability;
        GetComponent<CapsuleCollider>().radius = radius;

    }
    void Awake()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider collider in colliders)
            HandleCollision(collider);
    }


    private void OnTriggerEnter(Collider collision)
    {
        HandleCollision(collision);
    }
    private void HandleCollision(Collider collision)
    {
        if (collision.gameObject.TryGetComponent(out LivingBeing livingBeing))
        {
            if (Ability.ThoroughIsUsableOn(Conjurer, livingBeing))
                CombatStatHandler.HandleEffectPackage(Ability, Conjurer, livingBeing, Ability.TargetEffects);
        }
    }
}
