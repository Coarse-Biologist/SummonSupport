using UnityEngine;

public static class Logging
{
    public enum LogLevel { None, Error, Warning, Info, Verbose }

    public static LogLevel CurrentLogLevel { get; set; } = LogLevel.Info;

    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        if (level <= CurrentLogLevel)
        {
            Debug.Log($"[{level}] {message}");
        }
    }
    public static void Error(string message)
    {
        Log(message, LogLevel.Error);
    }

    public static void Warning(string message)
    {
        Log(message, LogLevel.Warning);
    }

    public static void Info(string message)
    {
        Log(message, LogLevel.Error);
    }

    public static void Verbose(string message)
    {
        Log(message, LogLevel.Verbose);
    }
}
