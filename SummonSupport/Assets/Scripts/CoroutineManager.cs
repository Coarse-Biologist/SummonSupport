using UnityEngine;
using System.Collections;

public class CoroutineManager : MonoBehaviour
{
    public static CoroutineManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Coroutine StartCustomCoroutine(IEnumerator routine)
    {
        if (routine == null)
        {
            Logging.Warning("Routine is null!");
            return null;
        }
        Logging.Info("Starting coroutine: " + routine);
        return StartCoroutine(routine);
    }
}