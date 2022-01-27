using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Watcher;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Skinner
{
    public class AssetSkinnerGUI
    {
        private static Vector2 scrollPosition;
        private static bool showSkinner = true;

        private static readonly string defaultIcon = "WelcomeScreen.AssetStoreLogo";
        private static readonly string errorIcon = "console.erroricon";
        private static readonly Dictionary<string, string> iconPerExtension = new Dictionary<string, string>()
        {
            {
                ".anim", "AnimationClip Icon"
            },
            {
                ".asset", "ScriptableObject Icon"
            },
            {
                ".controller", "AnimatorController Icon"
            },
            {
                ".cs", "cs Script Icon"
            },
            {
                ".gradle", "TextAsset Icon"
            },
            {
                ".json", "TextAsset Icon"
            },
            {
                ".prefab", "Prefab Icon"
            },
            {
                ".txt", "TextAsset Icon"
            },
            {
                ".unity", "SceneAsset Icon"
            },
            {
                ".xml", "TextAsset Icon"
            },
        };

        public static void OnGUI()
        {
            showSkinner = EditorGUILayout.Foldout(showSkinner, "Asset Skinner");

            if (!showSkinner)
            {
                return;
            }

            GUIStyle boxStyle = StyleUtils.BoxStyle;

            using (new GUILayout.VerticalScope(boxStyle))
            {
                GUILayout.Label("Selected assets:", EditorStyles.boldLabel);

                Object[] assets = Selection.GetFiltered<Object>(SelectionMode.Assets);

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                foreach (var assetSupportInfo in assets.GetAssetsSupportInfo())
                {
                    if (assetSupportInfo.isSupported)
                    {
                        DrawAssetSection(assetSupportInfo.asset);
                    }
                    else
                    {
                        DrawUnsupportedAssetSection(assetSupportInfo);
                    }
                }

                GUILayout.EndScrollView();
            }
        }

        private static void DrawUnsupportedAssetSection(
            (Object asset, bool isSupported, string reason) assetSupportInfo)
        {
            EditorGUILayout.InspectorTitlebar(true, assetSupportInfo.asset);
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.grey;
            GUILayout.Label($"Not supported: {assetSupportInfo.reason}");
            GUI.backgroundColor = oldColor;
        }

        private static void DrawAssetSection(Object asset)
        {
            EditorGUILayout.InspectorTitlebar(true, asset);

            string path = AssetDatabase.GetAssetPath(asset);

            bool skinned = AssetSkinner.IsSkinned(path);

            Color oldColor = GUI.backgroundColor;

            if (skinned)
            {
                DrawSkinnedAssetSection(path);
            }
            else
            {
                DrawUnskinnedAssetSection(path);
            }

            GUI.backgroundColor = oldColor;
        }

        internal static void DrawAssetPreview(string key, string game, string path)
        {
            GUIStyle boxStyle = StyleUtils.BoxStyle;

            using (new GUILayout.VerticalScope(boxStyle))
            {
                float buttonWidth = EditorGUIUtility.currentViewWidth
                                    * (1.0f / ShapeShifterConfiguration.Instance.GameNames.Count);
                buttonWidth -= 20; // to account for a possible scrollbar or some extra padding 

                bool clicked = false;
                if (ShapeShifter.CachedPreviewPerAssetDict.TryGetValue(key, out Texture2D previewImage))
                {
                    clicked = GUILayout.Button(
                        previewImage,
                        GUILayout.Width(buttonWidth),
                        GUILayout.MaxHeight(buttonWidth)
                    );
                }
                else
                {
                    clicked = GUILayout.Button(
                        "Missing Preview Image",
                        GUILayout.Width(buttonWidth),
                        GUILayout.MaxHeight(buttonWidth)
                    );
                }

                if (clicked)
                {
                    EditorUtility.RevealInFinder(path);
                }

                using (new GUILayout.HorizontalScope(boxStyle))
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(game);
                    GUILayout.FlexibleSpace();
                }
            }
        }

        private static void DrawSkinnedAssetSection(string assetPath)
        {
            GUIStyle boxStyle = StyleUtils.BoxStyle;

            using (new GUILayout.HorizontalScope(boxStyle))
            {
                foreach (string game in ShapeShifterConfiguration.Instance.GameNames)
                {
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);
                    string skinnedPath = Path.Combine(
                        ShapeShifter.SkinsFolder.FullName,
                        game,
                        ShapeShifter.INTERNAL_ASSETS_FOLDER,
                        guid,
                        Path.GetFileName(assetPath)
                    );

                    string key = ShapeShifterUtils.GenerateUniqueAssetSkinKey(game, guid);
                    GenerateAssetPreview(key, skinnedPath);
                    DrawAssetPreview(key, game, skinnedPath);
                }
            }

            GUI.backgroundColor = Color.red;

            if (GUILayout.Button("Remove skins"))
            {
                AssetSkinner.RemoveSkins(assetPath);
            }
        }

        private static void DrawUnskinnedAssetSection(string assetPath)
        {
            GUI.backgroundColor = Color.green;

            if (GUILayout.Button("Skin it!"))
            {
                AssetSkinner.SkinAsset(assetPath);
            }
        }

        internal static void GenerateAssetPreview(string key, string assetPath)
        {
            if (ShapeShifter.DirtyAssets.Contains(key) || !ShapeShifter.CachedPreviewPerAssetDict.ContainsKey(key))
            {
                ShapeShifter.DirtyAssets.Remove(key);

                Texture2D texturePreview = EditorGUIUtility.FindTexture(errorIcon);
                if (Directory.Exists(assetPath))
                {
                    texturePreview = EditorGUIUtility.FindTexture("Folder Icon");
                }
                else if (!File.Exists(assetPath))
                {
                    texturePreview = EditorGUIUtility.FindTexture(errorIcon);
                }
                else
                {
                    string extension = Path.GetExtension(assetPath);

                    if (IsValidImageFormat(extension))
                    {
                        texturePreview = new Texture2D(0, 0);
                        texturePreview.LoadImage(File.ReadAllBytes(assetPath));
                        string skinFolder = Directory.GetParent(assetPath).FullName;
                        AssetWatcher.StartWatchingFolder(skinFolder);
                    }
                    else
                    {
                        string icon = defaultIcon;

                        if (iconPerExtension.ContainsKey(extension))
                        {
                            icon = iconPerExtension[extension];
                        }

                        texturePreview = (Texture2D)EditorGUIUtility.IconContent(icon).image;
                    }
                }

                ShapeShifter.CachedPreviewPerAssetDict[key] = texturePreview;
            }
        }
        

        private static bool IsValidImageFormat(string extension) =>
            extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp";
    }
}