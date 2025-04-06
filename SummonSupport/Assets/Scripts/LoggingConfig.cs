using Unity.VisualScripting;
using UnityEngine;

public class LoggingConfig : MonoBehaviour
{
    [SerializeField] Logging.LogLevel logLevel = Logging.LogLevel.Info;
    void Start()
    {
        if (logLevel == Logging.LogLevel.None)
        {
            logLevel = Logging.LogLevel.Warning;
        }
        Logging.CurrentLogLevel = logLevel;
    }
}
