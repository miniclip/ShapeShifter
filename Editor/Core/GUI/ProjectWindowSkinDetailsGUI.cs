using Miniclip.ShapeShifter.Skinner;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    [InitializeOnLoad]
    public static class ProjectWindowSkinDetailsGUI
    {
        private const string overlayIconPath =
            "Packages/com.miniclip.unity.shapeshifter/Editor/Icons/shapeshifter_icon.png";
        private static Texture2D cachedOverlayIcon;

        static ProjectWindowSkinDetailsGUI()
        {
            cachedOverlayIcon = LoadOverlayIcon();

            if (ShapeShifterConfiguration.IsInitialized())
            {
                SubscribeToProjectWindowItemOnGUI();
            }
            else
            {
                ShapeShifterConfiguration.OnInitialised += SubscribeToProjectWindowItemOnGUI;
            }
        }

        private static void SubscribeToProjectWindowItemOnGUI()
        {
            EditorApplication.projectWindowItemOnGUI += DrawSkinIconOverlay;
        }

        private static void DrawSkinIconOverlay(string guid, Rect rect)
        {
            if (Application.isPlaying || Event.current.type != EventType.Repaint)
            {
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!AssetSkinner.IsSkinned(assetPath) && !AssetSkinner.TryGetParentSkinnedFolder(assetPath, out string _)) 
            {
                return;
            }

            if (cachedOverlayIcon == null)
            {
                return;
            }

            float iconSize = rect.height;

            rect.width = iconSize;
            rect.height = iconSize;

            GUI.DrawTexture(rect, cachedOverlayIcon);
        }

        private static Texture2D LoadOverlayIcon()
        {
            return (Texture2D) EditorGUIUtility.Load(overlayIconPath);
        }
    }
}