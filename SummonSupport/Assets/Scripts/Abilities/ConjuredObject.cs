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
        GetComponent<CircleCollider2D>().radius = radius;

    }
    void Awake()
    {
        Collider2D collider = Physics2D.OverlapCircle(transform.position, radius);
        HandleCollision(collider);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision);
    }
    private void HandleCollision(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out LivingBeing livingBeing))
        {
            if (Ability.ThoroughIsUsableOn(Conjurer, livingBeing))
                CombatStatHandler.HandleEffectPackages(Ability, Conjurer, livingBeing, false);
        }
    }
}
