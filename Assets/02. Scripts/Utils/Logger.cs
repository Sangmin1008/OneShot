using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OneShot
{
    public static class Logger
    {
        [Conditional("UNITY_EDITOR")]
        public static void Log(string message)
        {
            Debug.Log($"<color=green> [정보] </color> <color=white>{message}</color>");
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(string message)
        {
            Debug.Log($"<color=yellow> [경고] </color> <color=white>{message}</color>");
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void LogError(string message)
        {
            Debug.Log($"<color=red> [오류] </color> <color=white>{message}</color>");
        }
    }
}

