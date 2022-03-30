using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    internal enum PersistenceType { SessionPersistent, MachinePersistent }

    internal static class Persistence
    {
        private const string SHAPESHIFTER_KEY = "shapeshifter";

        private static string UniqueProjectID => Path.GetFullPath(Application.dataPath);

        internal static string GetProjectSpecificKey(string key) => $"{UniqueProjectID}_{SHAPESHIFTER_KEY}_{key}";

        internal static bool HasKey(string key, PersistenceType persistenceType)
        {
            if (persistenceType == PersistenceType.MachinePersistent)
            {
                return EditorPrefs.HasKey(GetProjectSpecificKey(key));
            }

            throw new ArgumentOutOfRangeException(
                nameof(persistenceType),
                persistenceType,
                "Method does not exist for SessionState"
            );
        }

        internal static int GetInt(string key, PersistenceType persistenceType)
        {
            switch (persistenceType)
            {
                case PersistenceType.MachinePersistent:
                    return EditorPrefs.GetInt(GetProjectSpecificKey(key));
                case PersistenceType.SessionPersistent:
                    return SessionState.GetInt(GetProjectSpecificKey(key), -1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(persistenceType), persistenceType, null);
            }
        }

        internal static void SetInt(string key, int value, PersistenceType persistenceType)
        {
            switch (persistenceType)
            {
                case PersistenceType.MachinePersistent:
                    SessionState.SetInt(GetProjectSpecificKey(key), value);
                    break;
                case PersistenceType.SessionPersistent:
                    EditorPrefs.SetInt(GetProjectSpecificKey(key), value);
                    break;
            }

            ShapeShifterLogger.Log($"Persistence: Setting new value in {key}");
        }

        public static bool GetBool(string key, PersistenceType persistenceType)
        {
            switch (persistenceType)
            {
                case PersistenceType.MachinePersistent:
                    return EditorPrefs.GetBool(GetProjectSpecificKey(key));
                case PersistenceType.SessionPersistent:
                    return SessionState.GetBool(GetProjectSpecificKey(key), false);
                default:
                    throw new ArgumentOutOfRangeException(nameof(persistenceType), persistenceType, null);
            }
        }

        public static void SetBool(string key, bool value, PersistenceType persistenceType)
        {
            switch (persistenceType)
            {
                case PersistenceType.MachinePersistent:
                    EditorPrefs.SetBool(GetProjectSpecificKey(key), value);
                    break;
                case PersistenceType.SessionPersistent:
                    SessionState.SetBool(GetProjectSpecificKey(key), value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(persistenceType), persistenceType, null);
            }

            ShapeShifterLogger.Log($"Persistence: Setting new value in {key}");
        }

        public static string GetString(string key, PersistenceType persistenceType)
        {
            switch (persistenceType)
            {
                case PersistenceType.MachinePersistent:
                    return EditorPrefs.GetString(GetProjectSpecificKey(key));
                case PersistenceType.SessionPersistent:
                    return SessionState.GetString(GetProjectSpecificKey(key), String.Empty);
                default:
                    throw new ArgumentOutOfRangeException(nameof(persistenceType), persistenceType, null);
            }
        }

        public static void SetString(string key, string value, PersistenceType persistenceType)
        {
            switch (persistenceType)
            {
                case PersistenceType.MachinePersistent:
                    EditorPrefs.SetString(GetProjectSpecificKey(key), value);
                    break;
                case PersistenceType.SessionPersistent:
                    SessionState.SetString(GetProjectSpecificKey(key), value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(persistenceType), persistenceType, null);
            }

            ShapeShifterLogger.Log($"Persistence: Setting new value in {key}");
        }
    }
}