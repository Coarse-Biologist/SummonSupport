using UnityEngine;

public class ViciousDeathExplosion : MonoBehaviour
{
    [field: SerializeField] public float ExplosionDamage { private set; get; } = 30;
    public void CheckTargetToExplodeOn(CharacterTag characterType)
    {
        Debug.Log("Vicious Death explosion taking place.");
        Collider[] hits = Physics.OverlapCapsule(transform.position + Vector3.up, transform.position - Vector3.up, 2);
        foreach (Collider hitObject in hits)
        {
            if (hitObject.TryGetComponent(out LivingBeing livingBeing))
            {
                if (livingBeing.CharacterTag == characterType)
                {
                    float relevantAffinity = livingBeing.Affinities[Element.Heat].Get();
                    if (relevantAffinity > 0)
                    {
                        ExplosionDamage -= ExplosionDamage * relevantAffinity / 100;
                    }

                    livingBeing.ChangeAttribute(AttributeType.CurrentHitpoints, -ExplosionDamage);
                }
            }
        }
    }


}
