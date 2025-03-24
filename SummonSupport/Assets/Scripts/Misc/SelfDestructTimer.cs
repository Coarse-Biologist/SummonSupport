using UnityEngine;
using System.Collections;

public class SelfDestructTimer : MonoBehaviour
{
    public float timeToDestroy = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartTimer()
    {
        StartCoroutine(DestroyAfterTime());
    }
    IEnumerator DestroyAfterTime()
    {
        // Warten für die angegebene Zeit
        yield return new WaitForSeconds(timeToDestroy);
        // Lösche das GameObject
        Destroy(gameObject);
    }
}
