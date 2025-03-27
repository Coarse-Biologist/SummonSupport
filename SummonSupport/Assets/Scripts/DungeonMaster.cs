using UnityEngine;

public class GameMaster : MonoBehaviour
{
    [SerializeField] Logging.LogLevel loggingLevel;
    AbilityHandler abilityHandler;

    void Start()
    {
        Logging.CurrentLogLevel = loggingLevel;
        Logging.Info("CurrentLogLevel: " + loggingLevel);
        abilityHandler = GameObject.FindGameObjectWithTag("Player").GetComponent<AbilityHandler>();        
    }

    void Update()
    {
        
    }
}
