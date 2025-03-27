using UnityEngine;
using System.Collections;

public class SelfDestructTimer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartTimer(float timeToDestroy)
    {
        StartCoroutine(DestroyAfterTime(timeToDestroy));
    }
    IEnumerator DestroyAfterTime(float timeToDestroy)
    {
        // Warten für die angegebene Zeit
        yield return new WaitForSeconds(timeToDestroy);
        // Lösche das GameObject
        Destroy(gameObject);
    }
}