using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    public class ShapeShifterLogger
    {
        private static readonly string SHAPESHIFTER_LOG_TAG = "ShapeShifter";
        
        internal static void Log(string message)
        {
            Debug.Log($"{SHAPESHIFTER_LOG_TAG}: {message}");
        }
        
        internal static void LogError(string message)
        {
            Debug.LogError($"{SHAPESHIFTER_LOG_TAG}: {message}");
        }
        
        internal static void LogWarning(string message)
        {
            Debug.LogWarning($"{SHAPESHIFTER_LOG_TAG}: {message}");
        }
    }
}