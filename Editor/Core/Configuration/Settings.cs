using Miniclip.ShapeShifter.Utils;
using UnityEditor;

namespace Miniclip.ShapeShifter
{
    class Settings
    {
        private const string ENABLE_AUTO_SAVE_KEY = "SETTINGS_ENABLE_AUTO_SAVE";

        internal static bool IsAutoSaveEnabled
        {
            get => Persistence.GetBool(ENABLE_AUTO_SAVE_KEY, PersistenceType.MachinePersistent);
            set => Persistence.SetBool(ENABLE_AUTO_SAVE_KEY, value, PersistenceType.MachinePersistent);
        }

        internal static void OnGUI()
        {
            IsAutoSaveEnabled = OnToggle("Auto Save", IsAutoSaveEnabled);
        }

        private static bool OnToggle(string label, bool boolValue)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                return EditorGUILayout.ToggleLeft(label, boolValue);
            }
        }
    }
}