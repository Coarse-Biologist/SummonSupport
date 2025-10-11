using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayBarrier : MonoBehaviour
{
    //private List<GameObject> RecentlyEnteredCreatures = new();

    private WaitForSeconds waitTime = new WaitForSeconds(1f);
    private List<Collider2D> triggersToDeactivate = new();

    void OnTriggerEnter2D(Collider2D collider)
    {
        //Debug.Log($"on exit function is called. the collider culprit was {collider}");
        if (collider.gameObject.TryGetComponent<EnemyStats>(out EnemyStats enemyStats))
        {
            Debug.Log($"setting ISTRigger is True");
            collider.isTrigger = true;
        }
        //else Debug.Log($"{collider.gameObject} does not have a charStats (it is {charStats})");
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        //Debug.Log($"on exit function is called. the collider culprit was {collider}");
        if (collider.gameObject.TryGetComponent<EnemyStats>(out EnemyStats enemyStats))
        {
            //triggersToDeactivate.Add(collider);
            StartCoroutine(DelayedTriggerDeactivation(collider));
        }
        //else Debug.Log($"{collider.gameObject} does not have a charStats (it is {charStats})");

    }
    private IEnumerator DelayedTriggerDeactivation(Collider2D collider)
    {
        Debug.Log($"ISTRigger is True");
        yield return waitTime;
        //if (triggersToDeactivate.Count > 0)
        //{
        //Debug.Log($"deactivating trigger of {triggersToDeactivate[0]}");
        if (collider != null)
        {
            Debug.Log($"Setting is trigger to false");
            collider.isTrigger = false;
        }

    }

}


