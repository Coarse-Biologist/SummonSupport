using UnityEditor.Rendering;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject avoid;
    public StatusEffect statusEffect; 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject != avoid)
        {
            statusEffect.ApplyStatusEffect(other.gameObject);
            Destroy(gameObject);
            Logging.Info($"projectile hit {other.name}");
        }
    }
}
