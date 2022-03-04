using System.Collections.Generic;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Saver;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Watcher;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Miniclip.ShapeShifter.Skinner
{
    public class AssetSkinnerGUI
    {
        private static Vector2 scrollPosition;

        private static readonly string defaultIcon = "WelcomeScreen.AssetStoreLogo";
        private static readonly string errorIcon = "console.erroricon";

        private static readonly Dictionary<string, string> iconPerExtension = new Dictionary<string, string>
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

        private static readonly Dictionary<object, bool> foldoutDictionary = new Dictionary<object, bool>();

        public static void OnGUI()
        {
            GUIStyle boxStyle = StyleUtils.BoxStyle;

            using (new GUILayout.VerticalScope(boxStyle))
            {
                GUILayout.Label("Selected assets:", EditorStyles.boldLabel);

                Object[] assets = Selection.GetFiltered<Object>(SelectionMode.Assets);

                using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
                {
                    scrollPosition = scrollView.scrollPosition;
                    foreach ((Object asset, bool isSupported, string reason) assetSupportInfo in
                             assets.GetAssetsSupportInfo())
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
                }
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
            if (asset == null)
            {
                return;
            }

            DraAssetHeaderGUI(asset);

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

            if (asset is GameObject prefab)
            {
                if (!foldoutDictionary.ContainsKey(asset))
                {
                    foldoutDictionary.Add(asset, false);
                }

                SpriteRenderer[] spriteRenderers = prefab.GetComponentsInChildren<SpriteRenderer>();
                Image[] images = prefab.GetComponentsInChildren<Image>();

                foldoutDictionary[asset] = EditorGUILayout.Foldout(foldoutDictionary[asset], "Prefab Contains", true);

                if (foldoutDictionary[asset])
                {
                    List<Texture2D> texture2Ds = new List<Texture2D>();

                    foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                    {
                        if (spriteRenderer.sprite == null)
                        {
                            continue;
                        }

                        texture2Ds.Add(spriteRenderer.sprite.texture);
                    }

                    foreach (Image image in images)
                    {
                        if (image.sprite == null)
                        {
                            continue;
                        }

                        texture2Ds.Add(image.sprite.texture);
                    }

                    foreach (Texture2D texture2D in texture2Ds.Distinct())
                    {
                        DrawAssetSection(texture2D);
                    }
                }
            }
        }

        private static void DraAssetHeaderGUI(Object asset)
        {
            if (EditorGUILayout.InspectorTitlebar(false, targetObj: asset, expandable: false))
            {
                EditorGUIUtility.PingObject(asset);
            }
        }

        internal static void DrawAssetPreview(string key, string game, string path, string guid,
            bool drawDropAreaToReplace = true)
        {
            GUIStyle boxStyle = StyleUtils.BoxStyle;

            using (new GUILayout.VerticalScope(boxStyle))
            {
                float buttonWidth = EditorGUIUtility.currentViewWidth
                                    * (1.0f / ShapeShifterConfiguration.Instance.GameNames.Count);
                buttonWidth -= 20; // to account for a possible scrollbar or some extra padding 

                bool clicked = false;
                EditorGUILayout.PrefixLabel(game);
                if (ShapeShifter.CachedPreviewPerAssetDict.TryGetValue(key, out Texture2D previewImage))
                {
                    clicked = GUILayout.Button(
                        previewImage,
                        GUILayout.Width(buttonWidth),
                        GUILayout.MaxHeight(buttonWidth)
                    );

                    AssetPreviewDropArea(game, path, guid, drawDropAreaToReplace);
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
            }
        }

        private static void AssetPreviewDropArea(string game, string path, string guid, bool drawDropAreaToReplace)
        {
            if (drawDropAreaToReplace && DropAreaGUI(out string replacementFilePath))
            {
                if (Path.GetExtension(replacementFilePath) == Path.GetExtension(path)
                    && !PathUtils.IsDirectory(replacementFilePath))
                {
                    FileUtils.SafeCopy(replacementFilePath, path);

                    if (ShapeShifter.ActiveGameName == game)
                    {
                        AssetSwitcher.RefreshAsset(guid);
                    }
                }
                else
                {
                    ShapeShifterLogger.LogWarning("Unable to replace asset.");
                }
            }
        }

        private static bool DropAreaGUI(out string filepath)
        {
            Event evt = Event.current;
            Rect dropAreaRect = GUILayoutUtility.GetLastRect();
            GUILayout.Box("Drag file to replace or click here to open file panel");
            Rect boxRect = GUILayoutUtility.GetLastRect();
            filepath = string.Empty;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropAreaRect.Contains(evt.mousePosition))
                    {
                        return false;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (string dragged_path in DragAndDrop.paths)
                        {
                            filepath = dragged_path;
                            return true;
                        }
                    }

                    break;
                case EventType.MouseDown:
                    if (boxRect.Contains(evt.mousePosition))
                    {
                        filepath = EditorUtility.OpenFilePanel("File Panel", Application.dataPath, "");
                        return true;
                    }

                    break;
            }

            return false;
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
                        ShapeShifterConstants.INTERNAL_ASSETS_FOLDER,
                        guid,
                        Path.GetFileName(assetPath)
                    );

                    string key = ShapeShifterUtils.GenerateUniqueAssetSkinKey(game, guid);
                    GenerateAssetPreview(key, skinnedPath);
                    DrawAssetPreview(key, game, skinnedPath, guid);
                }
            }

            GUI.backgroundColor = Color.red;

            if (GUILayout.Button("Remove skins"))
            {
                AssetSkinner.RemoveSkins(assetPath);
                GUIUtility.ExitGUI();
            }
        }

        private static void DrawUnskinnedAssetSection(string assetPath)
        {
            GUI.backgroundColor = Color.green;

            if (GUILayout.Button("Skin it!"))
            {
                AssetSkinner.SkinAsset(assetPath);
                GUIUtility.ExitGUI();
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

                        texturePreview = (Texture2D) EditorGUIUtility.IconContent(icon).image;
                    }
                }

                ShapeShifter.CachedPreviewPerAssetDict[key] = texturePreview;
            }
        }

        private static bool IsValidImageFormat(string extension) =>
            extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp";
    }
}