using UnityEngine;
using System.Diagnostics;
public static class Logging
{
    public enum LogLevel { None, Error, Warning, Info, Verbose }

    public static LogLevel CurrentLogLevel { get; set; } = LogLevel.Info;

    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        if (level <= CurrentLogLevel)
        {
            StackTrace stackTrace = new StackTrace(1, true);
            StackFrame frame = stackTrace.GetFrame(0);
            string callerInfo = frame != null ? $"{frame.GetFileName()}:{frame.GetFileLineNumber()}" : "Unknown";
            UnityEngine.Debug.Log($"[{level}] {message}\n{callerInfo}");
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
        Log(message, LogLevel.Info);
    }

    public static void Verbose(string message)
    {
        Log(message, LogLevel.Verbose);
    }
}
