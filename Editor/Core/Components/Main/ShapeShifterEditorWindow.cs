using System;
using System.IO;
using System.Reflection;
using Miniclip.ShapeShifter.Saver;
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
        private int selectedTab;

        private enum TabOptions
        {
            AssetSkinner = 0,
            ExternalSkinner = 1,
            Configuration = 2,
            Tools = 3
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

        private void OnSelectionChange()
        {
            ShapeShifter.DirtyAssets.Clear();
            ShapeShifter.CachedPreviewPerAssetDict.Clear();
            AssetWatcher.ClearAllWatchedPaths();
        }

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

            AssetSwitcherGUI.OnGUI();

            selectedTab = GUILayout.Toolbar(
                selectedTab,
                Enum.GetNames(typeof(TabOptions))
            );
            Debug.Log(selectedTab);

            switch ((TabOptions) selectedTab)
            {
                case TabOptions.AssetSkinner:
                    AssetSkinnerGUI.OnGUI();
                    break;
                case TabOptions.ExternalSkinner:
                    ExternalAssetSkinnerGUI.OnGUI();
                    break;
                case TabOptions.Configuration:
                    OnShowConfigurationGUI();
                    break;
                case TabOptions.Tools:
                    OnShowUtilOperationsGUI();
                    OnDangerousOperationsGUI();
                    ShapeShifterLogger.OnGUI();
                    break;
            }

            using (new GUILayout.VerticalScope())
            {
                OnShowComponentsGUI();
            }

            Repaint();
        }

        private void OnShowConfigurationGUI()
        {
            if (ShapeShifterConfiguration.Instance.DefaultConfigurationEditor != null
                && ShapeShifterConfiguration.Instance.ExternalConfigurationEditor != null)
            {
                ShapeShifterConfiguration.Instance.DefaultConfigurationEditor.OnInspectorGUI();
                ShapeShifterConfiguration.Instance.ExternalConfigurationEditor.OnInspectorGUI();
            }
        }

        private static void OnShowComponentsGUI() { }

        private static void OnShowUtilOperationsGUI()
        {
            if (GUILayout.Button($"Force Save To {ShapeShifter.ActiveGameName}", StyleUtils.ButtonStyle))
            {
                AssetSaver.SaveToActiveGameSkin();
            }

            GUILayout.Space(30);

            if (GUILayout.Button("Restore Missing Assets"))
            {
                AssetSwitcher.RestoreMissingAssets();
            }

            PreMergeCheckGUI.OnGUI();
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
            if (GUILayout.Button("Remove all skins") && RemoveAllSkinsDisplayDialog())
            {
                EditorUtility.DisplayProgressBar("ShapeShifter", "Deleting internal skins", 0.0f);
                AssetSkinner.RemoveAllInternalSkins();
                EditorUtility.DisplayProgressBar("ShapeShifter", "Deleting external skins", 0.3f);
                ExternalAssetSkinner.RemoveAllExternalSkins();
                EditorUtility.DisplayProgressBar("ShapeShifter", "Cleaning up git ignore", 0.6f);
                GitIgnore.ClearShapeShifterEntries();
                EditorUtility.DisplayProgressBar("ShapeShifter", "Deleting main skins folder", 1f);
                if (Directory.Exists(ShapeShifter.SkinsFolder.FullName))
                {
                    ShapeShifter.SkinsFolder.Delete(true);
                }

                EditorUtility.ClearProgressBar();
                GUIUtility.ExitGUI();
            }

            GUI.backgroundColor = backgroundColor;
        }

        private static bool RemoveAllSkinsDisplayDialog()
        {
            return EditorUtility.DisplayDialog(
                "ShapeShifter",
                $"You are about to remove shapeshifter's skin folders.\n Your project assets will remain "
                + $"the same as the current game skin ({ShapeShifter.ActiveGameName}).\n You will loose the other game skins",
                "Continue",
                "Cancel"
            );
        }
    }
}