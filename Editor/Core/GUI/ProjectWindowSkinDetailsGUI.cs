using Miniclip.ShapeShifter;
using Miniclip.ShapeShifter.Skinner;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ProjectWindowSkinDetailsGUI
{
    private static Texture2D diamondOverlayTexture;

    static ProjectWindowSkinDetailsGUI()
    {
        diamondOverlayTexture = LoadDiamondIcon();

        EditorApplication.projectWindowItemOnGUI += DrawSkinIconOverlay;
    }

    private static void DrawSkinIconOverlay(string guid, Rect rect)
    {
        if (Application.isPlaying || Event.current.type != EventType.Repaint)
        {
            return;
        }
        
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        
        if (!AssetSkinner.IsSkinned(assetPath))
            return;

        if (diamondOverlayTexture == null)
            return;

        float iconSize = rect.height / 3f;
        float positionOffset = iconSize / 4f;
        rect.position -= new Vector2(positionOffset, positionOffset);
        rect.width = iconSize;
        rect.height = iconSize;
        
        GUI.DrawTexture(rect, diamondOverlayTexture);
    }
    
    
    public static Texture2D LoadDiamondIcon()
    {
        return (Texture2D) EditorGUIUtility.Load("Packages/com.miniclip.unity.shapeshifter/Editor/Icons/diamond_shapeshifter.png");
    }
}