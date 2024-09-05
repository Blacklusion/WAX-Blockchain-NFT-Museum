using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if RELEASE
public static class Debug
{
    public static void Log(object message)
    {
    }

    public static void Log(object message, Object context)
    {
    }

    public static void LogError(object message)
    {
    }

    public static void LogError(object message, Object context)
    {
    }

    public static void LogWarning(object message)
    {
    }

    public static void LogWarning(object message, Object context)
    {
    }

    public static void LogException(System.Exception exception)
    {
    }

    public static void LogException(System.Exception exception, Object context)
    {
    }

    public static void LogFormat(string format, params object[] args)
    {
    }

    public static void LogFormat(Object context, string format, params object[] args)
    {
    }

    public static void LogFormat(LogType logType, LogOption logOptions, Object context, string format,
        params object[] args)
    {
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
    }

    public static void LogErrorFormat(Object context, string format, params object[] args)
    {
    }

    public static void LogWarningFormat(string format, params object[] args)
    {
    }

    public static void LogWarningFormat(Object context, string format, params object[] args)
    {
    }
}
#endif