using System;
using System.IO;
using System.Reflection;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Utils.Git;
using Miniclip.ShapeShifter.Watcher;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    class ShapeShifterEditorWindow : EditorWindow
    {
        private bool showConfiguration;

        private void OnGUI()
        {
            if (!ShapeShifterConfiguration.IsInitialized())
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Shapeshifter configuration needs to be initialized.");

                    if (GUILayout.Button("Initialize"))
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
                    ShapeShifterConfiguration.Instance.ExternalConfigurationEditor.OnInspectorGUI();
                }

                AssetSwitcherGUI.OnGUI();
                AssetSkinnerGUI.OnGUI();
                ExternalAssetSkinnerGUI.OnGUI();
                PreMergeCheckGUI.OnGUI();

                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Restore missing assets"))
                {
                    AssetSwitcher.RestoreMissingAssets();
                }

                OnDangerousOperationsGUI();
            }

            Repaint();
        }

        private void OnSelectionChange()
        {
            ShapeShifter.DirtyAssets.Clear();
            ShapeShifter.CachedPreviewPerAssetDict.Clear();
            AssetWatcher.ClearAllWatchedPaths();
        }

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

        private static void OnDangerousOperationsGUI()
        {
            GUILayout.BeginVertical(StyleUtils.BoxStyle);
            GUILayout.Label("Dangerous Operations");

            // AssetSwitcherGUI.OnOverwriteAllSkinsGUI();
            GUILayout.Space(20);
            OnRemoveAllSkinsGUI();
            GUILayout.EndVertical();
        }

        private static void OnRemoveAllSkinsGUI()
        {
            Color backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove all skins"))
            {
                AssetSkinner.RemoveAllInternalSkins();
                ExternalAssetSkinner.RemoveAllExternalSkins();
                GitIgnore.ClearShapeShifterEntries();
                if (ShapeShifter.SkinsFolder.Exists)
                {
                    ShapeShifter.SkinsFolder.Delete(true);
                }
            }

            GUI.backgroundColor = backgroundColor;
        }
    }
}