using UnityEditor.Rendering;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject avoid;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject != avoid)
        {
            Destroy(gameObject);
            Logging.Info($"projectile hit {other.name}");
        }
    }
}
