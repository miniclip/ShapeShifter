using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    internal static class ShapeShifterLogger
    {
        private static readonly string SHAPESHIFTER_LOG_TAG = "ShapeShifter";
        private static int MAX_LOG_ENTRIES = 15;

        private static Queue<string> loggedMessages = new Queue<string>();
        private static Vector2 scrollPosition;
        private static bool showLogs;


        internal static void Log(string message)
        {
            Debug.Log($"{SHAPESHIFTER_LOG_TAG}: {message}");
            AddToLoggedMessages(message);
        }

        internal static void LogError(string message)
        {
            Debug.LogError($"{SHAPESHIFTER_LOG_TAG}: {message}");
            AddToLoggedMessages(message);
        }

        internal static void LogWarning(string message)
        {
            Debug.LogWarning($"{SHAPESHIFTER_LOG_TAG}: {message}");
            AddToLoggedMessages(message);
        }

        private static void AddToLoggedMessages(string message)
        {
            loggedMessages.Enqueue(message);

            if (loggedMessages.Count > MAX_LOG_ENTRIES)
            {
                loggedMessages.Dequeue();
            }
        }

        internal static void OnGUI()
        {
            showLogs = EditorGUILayout.Foldout(showLogs, "Logs");

            if (!showLogs)
            {
                return;
            }

            using (new GUILayout.VerticalScope(StyleUtils.BoxStyle))
            {
                OnLoggedMessagesGUI();
            }
        }

        private static void OnLoggedMessagesGUI()
        {
            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                foreach (string loggedMessage in loggedMessages)
                {
                    GUILayout.Label(loggedMessage, StyleUtils.LabelStyle);
                }
            }
        }
    }
}