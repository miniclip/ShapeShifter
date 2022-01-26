using System;
using System.Reflection;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Watcher;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    class ShapeShifterEditorWindow : EditorWindow
    {
        private bool showConfiguration;

        [MenuItem("Window/Shape Shifter/Open ShapeShifter Window", false, 'G')]
        public static void OpenShapeShifter()
        {
            ShowNextToInspector(true);
        }

        private static void ShowNextToInspector(bool focus = false)
        {
            Assembly editorAssembly = typeof(Editor).Assembly;
            Type inspectorWindowType = editorAssembly.GetType("UnityEditor.InspectorWindow");

            GetWindow<ShapeShifterEditorWindow>(
                "Shape Shifter",
                focus,
                inspectorWindowType
            );
        }

        private void OnGUI()
        {
            if (ShapeShifterConfiguration.Instance == null)
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Shapeshifter configuration not found.");

                    if (GUILayout.Button("Try To Fix"))
                    {
                        ShapeShifterConfiguration.Initialise();
                    }

                    Repaint();
                    return;
                }
            }

            using (new GUILayout.VerticalScope())
            {
                showConfiguration = EditorGUILayout.Foldout(showConfiguration, "Configuration");

                if (showConfiguration
                    && ShapeShifterConfiguration.Instance.DefaultConfigurationEditor != null
                    && ShapeShifterConfiguration.Instance.ExternalConfigurationEditor != null)
                {
                    ShapeShifterConfiguration.Instance.DefaultConfigurationEditor.OnInspectorGUI();

                    // TODO: hide this when it's no longer necessary, as direct access to this list may cause issues
                    ShapeShifterConfiguration.Instance.ExternalConfigurationEditor.OnInspectorGUI();
                }

                AssetSwitcherGUI.OnGUI();
                AssetSkinnerGUI.OnGUI();
                ExternalAssetSkinnerGUI.OnGUI();

                if (GUILayout.Button("Restore missing assets"))
                {
                    AssetSwitcher.RestoreMissingAssets();
                }

                if (GUILayout.Button("Remove all skins"))
                {
                    var assetSkins = ShapeShifter.ActiveGameSkin.GetAssetSkins();
                    foreach (AssetSkin assetSkin in assetSkins)
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(assetSkin.Guid);
                        AssetSkinner.RemoveSkins(assetPath);
                    }
                }
                
                GUILayout.FlexibleSpace();
            }

            Repaint();
        }

        private void OnSelectionChange()
        {
            ShapeShifter.DirtyAssets.Clear();
            ShapeShifter.CachedPreviewPerAssetDict.Clear();
            AssetWatcher.ClearAllWatchedPaths();
        }
    }
}