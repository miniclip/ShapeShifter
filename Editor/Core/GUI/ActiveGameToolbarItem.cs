#if UNITY_2019
using System;
using System.Reflection;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miniclip.ShapeShifter
{
    [InitializeOnLoad]
    public static class ActiveGameToolbarItem
    {
        private static readonly Type toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static readonly Type guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");

        private static readonly PropertyInfo viewVisualTree = guiViewType.GetProperty(
            "visualTree",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        );

        private static readonly FieldInfo imguiContainerOnGui = typeof(IMGUIContainer).GetField(
            "m_OnGUIHandler",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        );

        private const float playPauseStopWidth = 140;
        private const float elementsSpacing = 10;

        private static ScriptableObject toolbar;

        static ActiveGameToolbarItem()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (toolbar == null)
            {
                toolbar = GetToolbar();

                if (toolbar == null)
                {
                    return;
                }

                VisualElement visualTree = (VisualElement)viewVisualTree.GetValue(toolbar, null);

                IMGUIContainer container = (IMGUIContainer)visualTree[0];

                Action handler = (Action)imguiContainerOnGui.GetValue(container);
                handler -= OnGUI;
                handler += OnGUI;
                imguiContainerOnGui.SetValue(container, handler);
            }
        }

        private static void OnGUI()
        {
            Rect rightRect = GetRect();

            GUILayout.BeginArea(rightRect);

            string toolbarButtonMessage;
            if (!ShapeShifterConfiguration.IsInitialized())
            {
                toolbarButtonMessage = "ShapeShifter";
            }
            else
            {
                toolbarButtonMessage = $"Active Game: {ShapeShifter.ActiveGameName.ToUpper()}";
            }

            DrawToolbarItem(toolbarButtonMessage, ShapeShifterEditorWindow.OpenShapeShifter);

            GUILayout.EndArea();
        }

        private static void DrawToolbarItem(string contentMessage, Action buttonPressedAction)
        {
            GUIStyle style = new GUIStyle(StyleUtils.ButtonStyle);
            style.fontStyle = FontStyle.Bold;
            GUIContent content = new GUIContent(contentMessage);

            style.CalcMinMaxWidth(content, out float _, out float maxWidth);

            style.fixedWidth = maxWidth;

            if (GUILayout.Button(content, style))
            {
                buttonPressedAction();
            }
        }

        private static Rect GetRect()
        {
            float screenWidth = EditorGUIUtility.currentViewWidth;

            Rect rightRect = new Rect(0, 0, screenWidth, Screen.height);
            int playPausePositionX = Mathf.RoundToInt((screenWidth - playPauseStopWidth) / 2f);

            rightRect.xMin = playPausePositionX + playPauseStopWidth + elementsSpacing;
            rightRect.yMin = 6;
            return rightRect;
        }

        private static ScriptableObject GetToolbar()
        {
            ScriptableObject[] toolbars = (ScriptableObject[])Resources.FindObjectsOfTypeAll(toolbarType);
            if (toolbars == null || toolbars.Length == 0)
            {
                return null;
            }

            return toolbars[0];
        }
    }
}
#endif