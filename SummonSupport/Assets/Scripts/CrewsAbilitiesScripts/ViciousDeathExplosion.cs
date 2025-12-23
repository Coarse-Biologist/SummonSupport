using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ViciousDeathExplosion : MonoBehaviour
{
    [field: SerializeField] public float ExplosionDamage { private set; get; } = 30;
    private List<LivingBeing> livingBeings = new();
    public void CheckTargetToExplodeOn(CharacterTag characterType)
    {
        Collider[] hits = Physics.OverlapCapsule(transform.position + Vector3.up, transform.position - Vector3.up, 10);
        foreach (Collider hitObject in hits)
        {
            if (hitObject.TryGetComponent(out LivingBeing livingBeing))
            {
                if (!livingBeings.Contains(livingBeing))
                {
                    livingBeings.Add(livingBeing);
                    if (livingBeing.CharacterTag == characterType)
                    {
                        if (livingBeing.GetAttribute(AttributeType.CurrentHitpoints) > 0)
                        {
                            ExplosionDamage -= ExplosionDamage * livingBeing.GetAffinity(Element.Heat) / 100;
                            livingBeing.ChangeAttribute(AttributeType.CurrentHitpoints, -ExplosionDamage);
                        }

                    }
                }
            }
        }
        Destroy(gameObject);
    }


}
