using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    class ShapeShifterEditorWindow : EditorWindow
    {
        private bool showConfiguration = false;

        [MenuItem("Window/Shape Shifter2/Open ShapeShifter Window", false, (int) 'G')]
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

                if (showConfiguration && ShapeShifterConfiguration.Instance.DefaultConfigurationEditor != null && ShapeShifterConfiguration.Instance.ExternalConfigurationEditor != null)
                {
                    ShapeShifterConfiguration.Instance.DefaultConfigurationEditor.OnInspectorGUI();
                    
                    // TODO: hide this when it's no longer necessary, as direct access to this list may cause issues
                    ShapeShifterConfiguration.Instance.ExternalConfigurationEditor.OnInspectorGUI();
                }

                // OnAssetSwitcherGUI();
                // OnAssetSkinnerGUI();
                // OnExternalAssetSkinnerGUI();
                //
                // if (GUILayout.Button("Restore missing assets"))
                // {
                //     RestoreMissingAssets();
                // }

                GUILayout.FlexibleSpace();
            }

            Repaint();
        }
    }
}