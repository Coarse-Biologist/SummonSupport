using SummonSupportEvents;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerMovement>(out PlayerMovement pm))
            EventDeclarer.SpawnEnemies?.Invoke(transform.parent.GetComponent<SpawnLocationInfo>());
    }
}
