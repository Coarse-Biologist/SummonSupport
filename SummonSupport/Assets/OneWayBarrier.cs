using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayBarrier : MonoBehaviour
{
    //private List<GameObject> RecentlyEnteredCreatures = new();

    private WaitForSeconds waitTime = new WaitForSeconds(1f);
    private List<Collider2D> triggersToDeactivate = new();

    void OnTriggerExit2D(Collider2D collider)
    {
        //Debug.Log($"on exit function is called. the collider culprit was {collider}");
        if (collider.gameObject.TryGetComponent<EnemyStats>(out EnemyStats charStats))
        {
            triggersToDeactivate.Add(collider);
            Invoke("DelayedTriggerDeactivation", 1f);
        }
        //else Debug.Log($"{collider.gameObject} does not have a charStats (it is {charStats})");

    }
    private void DelayedTriggerDeactivation()
    {
        if (triggersToDeactivate.Count > 0)
        {
            //Debug.Log($"deactivating trigger of {triggersToDeactivate[0]}");
            triggersToDeactivate[0].isTrigger = false;
            //triggersToDeactivate[0].gameObject.GetComponent<AIStateHandler>().SetTarget();
            triggersToDeactivate.Remove(triggersToDeactivate[0]);

        }
        //else Debug.Log("Dont overreach, young blood");

    }
}

