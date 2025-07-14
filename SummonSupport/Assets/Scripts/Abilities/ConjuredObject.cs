using UnityEngine;

public class ConjuredObject : MonoBehaviour
{
    public ConjureAbility Ability { get; private set; }
    public LivingBeing Conjurer { get; private set; }

    public void SetAbility(Ability ability)
    {
        ability = Ability;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out LivingBeing livingBeing))
        {
            if (Ability.IsUsableOn(Conjurer.CharacterTag, livingBeing.CharacterTag))
            {
                Logging.Info($"Conjuration ability is causing {Ability.Value} damage to {livingBeing.Name}");
                livingBeing.ChangeAttribute(Ability.Attribute, Ability.Value);
            }
        }
    }
}
