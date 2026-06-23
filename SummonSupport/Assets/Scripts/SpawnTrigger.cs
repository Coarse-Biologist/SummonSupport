using SummonSupportEvents;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    [field: SerializeField] public bool TriggerAutomatically;
    [field: SerializeField] public int TriggerTimer;
    private bool triggered = false;

    void Start()
    {
        if (TriggerAutomatically)
            Invoke("TriggerRepeatedly", TriggerTimer);
    }
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerMovement pm))
        {
            Trigger();
        }
    }
    private void TriggerRepeatedly()
    {
        Trigger(true);
    }
    private void Trigger(bool repeatTrigger = false)
    {
        if (repeatTrigger) triggered = false;
        if (!triggered)
        {
            EventDeclarer.SpawnEnemies?.Invoke(transform.parent.GetComponentInChildren<SpawnLocationInfo>());
            triggered = true;
            Destroy(gameObject);
        }
    }

}
