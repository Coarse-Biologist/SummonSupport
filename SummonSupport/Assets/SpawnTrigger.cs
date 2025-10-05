using SummonSupportEvents;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerMovement>(out PlayerMovement pm))
            EventDeclarer.SpawnEnemies?.Invoke(transform.parent.GetComponent<SpawnLocationInfo>());
    }
}
