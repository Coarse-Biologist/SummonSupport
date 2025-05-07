using UnityEngine;

public class SelfDestructTimer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void StartTimer(float time)
    {
        Logging.Info("Self destruct in" + time.ToString());
        Destroy(gameObject);
    }
}
