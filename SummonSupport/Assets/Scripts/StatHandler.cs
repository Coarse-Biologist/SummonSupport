using UnityEngine;

public class StatHandler : MonoBehaviour
{
    int hitPoints;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hitPoints = 50;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddHealth(int value)
    {
        hitPoints += value;
        Logging.Verbose("Healed: " + value + "\nHitpoints : " + hitPoints);
    }
}
