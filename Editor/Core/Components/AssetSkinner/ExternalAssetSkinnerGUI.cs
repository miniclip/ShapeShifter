using System.IO;
using Miniclip.ShapeShifter.Saver;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Skinner
{
    public static class ExternalAssetSkinnerGUI
    {
        private static int selectedExternalAsset;

        private static bool showExternalSkinner = true;

        private static void DrawSkinnedExternalAssetSection(string relativePath)
        {
            GUIStyle boxStyle = StyleUtils.BoxStyle;

            using (new GUILayout.HorizontalScope(boxStyle))
            {
                foreach (string game in ShapeShifterConfiguration.Instance.GameNames)
                {
                    string key = ExternalAssetSkinner.GenerateKeyFromRelativePath(relativePath);
                    string assetSkinPath = Path.Combine(
                        ShapeShifter.SkinsFolder.FullName,
                        game,
                        ShapeShifterConstants.EXTERNAL_ASSETS_FOLDER,
                        key,
                        Path.GetFileName(relativePath)
                    );

                    AssetSkinnerGUI.GenerateAssetPreview(key, assetSkinPath);
                    AssetSkinnerGUI.DrawAssetPreview(key, game, assetSkinPath, null, false);
                }
            }

            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;

            if (GUILayout.Button("Remove skins"))
            {
                ExternalAssetSkinner.RemoveExternalSkins(relativePath);
            }

            GUI.backgroundColor = oldColor;
        }

        internal static void OnGUI()
        {
            GUIStyle boxStyle = StyleUtils.BoxStyle;
            GUIStyle buttonStyle = StyleUtils.ButtonStyle;

            using (new GUILayout.VerticalScope(boxStyle))
            {
                int count = ShapeShifterConfiguration.Instance.SkinnedExternalAssetPaths.Count;

                if (count > 0)
                {
                    selectedExternalAsset = GUILayout.SelectionGrid(
                        selectedExternalAsset,
                        ShapeShifterConfiguration.Instance.SkinnedExternalAssetPaths.ToArray(),
                        2,
                        buttonStyle
                    );

                    if (selectedExternalAsset >= 0 && selectedExternalAsset < count)
                    {
                        string relativePath =
                            ShapeShifterConfiguration.Instance.SkinnedExternalAssetPaths[selectedExternalAsset];
                        DrawSkinnedExternalAssetSection(relativePath);
                    }
                }

                if (GUILayout.Button("Skin external file"))
                {
                    ExternalAssetSkinner.SkinExternalFile();
                }
            }
        }
    }
}