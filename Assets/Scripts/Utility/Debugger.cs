using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : Singleton<Debugger>
{
    public static void PrintLog(string log, LogType logType = LogType.Log)
    {
        #if UNITY_EDITOR
        switch (logType)
        {
            case LogType.Error:
                Debug.LogError(log);
                break;

            case LogType.Assert:
                Debug.LogAssertion(log);
                break;

            case LogType.Log:
                Debug.Log(log);
                break;

            case LogType.Warning:
                Debug.LogWarning(log);
                break;
        }
        #endif
    }
}
