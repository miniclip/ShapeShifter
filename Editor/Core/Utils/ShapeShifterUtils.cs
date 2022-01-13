using Miniclip.ShapeShifter.Switcher;
using UnityEditor;

namespace Miniclip.ShapeShifter.Utils
{
    internal static class ShapeShifterUtils
    {
        internal static string GenerateUniqueAssetSkinKey(string game, string guid) => game + ":" + guid;

        internal static string GetGameName(int index) => ShapeShifterConfiguration.Instance.GameNames[index];

        internal static void SavePendingChanges()
        {
            AssetDatabase.SaveAssets();

            // since the above doesn't seem to work with ScriptableObjects, might as well just go for a full save
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }

    }
}