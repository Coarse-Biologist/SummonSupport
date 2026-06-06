using SummonSupportEvents;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    private bool triggered = false;
    void OnTriggerEnter(Collider collision)
    {
        if (!triggered)
            if (collision.gameObject.TryGetComponent(out PlayerMovement pm))
            {
                EventDeclarer.SpawnEnemies?.Invoke(transform.parent.GetComponent<SpawnLocationInfo>());
                triggered = true;
                Destroy(gameObject);
            }
    }
}
