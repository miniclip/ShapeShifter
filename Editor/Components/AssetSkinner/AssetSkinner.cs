using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Miniclip.ShapeShifter {
   
    public static class AssetSkinner {
        
        private static Vector2 scrollPosition;
        private static bool showSkinner = true;

        internal static Dictionary<string, Texture2D> PreviewPerAsset = new Dictionary<string, Texture2D>();
        
        private static readonly string defaultIcon = "WelcomeScreen.AssetStoreLogo";
        private static readonly string errorIcon = "console.erroricon";
        private static readonly Dictionary<string, string> iconPerExtension = new Dictionary<string, string>() {
            {".anim", "AnimationClip Icon"},
            {".asset", "ScriptableObject Icon"},
            {".controller", "AnimatorController Icon"},
            {".cs", "cs Script Icon"},
            {".gradle", "TextAsset Icon"},
            {".json", "TextAsset Icon"},
            {".prefab", "Prefab Icon"},
            {".txt", "TextAsset Icon"},
            {".unity", "SceneAsset Icon"},
            {".xml", "TextAsset Icon"},
        };
        
#region GUI
        public static void OnAssetSkinnerGUI() {
            showSkinner = EditorGUILayout.Foldout(showSkinner, "Asset Skinner");

            if (! showSkinner) {
                return;
            }

            GUIStyle boxStyle = GUI.skin.GetStyle("Box");

            using (new GUILayout.VerticalScope(boxStyle))
            {
                GUILayout.Label("Selected assets:", EditorStyles.boldLabel);

                Object[] assets = Selection.GetFiltered<Object>(SelectionMode.Assets);
                
                List<Object> supportedAssets = assets.GetSupportedAssetsFromArray();
                
                if (supportedAssets.Count == 0)
                {
                    GUILayout.Label("None.");
                }
                else
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                    foreach (Object asset in supportedAssets)
                    {
                        DrawAssetSection(asset);
                    }

                    GUILayout.EndScrollView();
                }
            }
        }
        
        private static void DrawAssetSection(Object asset) {
            EditorGUILayout.InspectorTitlebar(true, asset);
            
            string path = AssetDatabase.GetAssetPath(asset);

            bool skinned = IsSkinned(path);

            Color oldColor = GUI.backgroundColor;
            
            if (skinned) {
                DrawSkinnedAssetSection(path);
            } else {
                DrawUnskinnedAssetSection(path);
            }
            
            GUI.backgroundColor = oldColor;
        }

        internal static void DrawAssetPreview(string key, string game, string path) {
            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            
            using (new GUILayout.VerticalScope(boxStyle)) {
                float buttonWidth = EditorGUIUtility.currentViewWidth * (1.0f / ShapeShifterConfiguration.Instance.GameNames.Count);
                buttonWidth -= 20; // to account for a possible scrollbar or some extra padding 

                bool clicked = false;
                if (PreviewPerAsset.TryGetValue(key, out Texture2D previewImage))
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
                
                
                if (clicked) {
                    EditorUtility.RevealInFinder(path);
                }

                using (new GUILayout.HorizontalScope(boxStyle)) {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(game);
                    GUILayout.FlexibleSpace();
                }
            }
        }

        private static void DrawSkinnedAssetSection(string assetPath) {
            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            
            using (new GUILayout.HorizontalScope(boxStyle)) {
                foreach (string game in ShapeShifterConfiguration.Instance.GameNames) {
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);
                    string skinnedPath = Path.Combine(
                        SharedInfo.SkinsFolder.FullName,
                        game,
                        SharedInfo.InternalAssetsFolder,
                        guid,
                        Path.GetFileName(assetPath)
                    );

                    string key = ShapeShifterUtils.GenerateUniqueAssetSkinKey(game, guid);
                    GenerateAssetPreview(key, skinnedPath);
                    DrawAssetPreview(key, game, skinnedPath);
                }
            }

            GUI.backgroundColor = Color.red;
                
            if (GUILayout.Button("Remove skins")) {
                RemoveSkins(assetPath);
            }        
        }

        private static void DrawUnskinnedAssetSection(string assetPath) {
            GUI.backgroundColor = Color.green;
                
            if (GUILayout.Button("Skin it!")) {
                SkinAsset(assetPath);
            }
        }

        internal static void GenerateAssetPreview(string key, string assetPath)
        {
            if (SharedInfo.DirtyAssets.Contains(key) || !PreviewPerAsset.ContainsKey(key))
            {
                SharedInfo.DirtyAssets.Remove(key);

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
                        ShapeShifter.StartWatchingFolder(skinFolder);
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

                PreviewPerAsset[key] = texturePreview;
            }
        }
        
#endregion

        
        
        private static void OnSelectionChange() {
            SharedInfo.DirtyAssets.Clear();
            PreviewPerAsset.Clear();
            ShapeShifter.ClearAllWatchedPaths();
        }

        public static void RemoveSkins(string assetPath) {
            foreach (string game in ShapeShifterConfiguration.Instance.GameNames) {
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                string key = ShapeShifterUtils.GenerateUniqueAssetSkinKey(game, guid);
                SharedInfo.DirtyAssets.Remove(key);
                PreviewPerAsset.Remove(key);
                
                string assetFolder = Path.Combine(
                    SharedInfo.SkinsFolder.FullName,
                    game,
                    SharedInfo.InternalAssetsFolder,
                    guid
                );

                ShapeShifter.StopWatchingFolder(assetFolder);
                Directory.Delete(assetFolder, true);
                GitUtils.Stage(assetFolder);
            }
            GitUtils.Track(assetPath);
        }

        private static void SkinAssets(string[] assetPaths, bool saveFirst = true)
        {
            if (saveFirst)
            {
                ShapeShifterUtils.SavePendingChanges();
            }

            foreach (string assetPath in assetPaths)
            {
                SkinAsset(assetPath);
            }
        }

        public static void SkinAsset(string assetPath, bool saveFirst = true)
        {
            if (saveFirst)
            {
                // make sure any pending changes are saved before generating copies
                ShapeShifterUtils.SavePendingChanges();
            }

            foreach (string game in ShapeShifterConfiguration.Instance.GameNames)
            {
                string origin = assetPath;
                string guid = AssetDatabase.AssetPathToGUID(origin);
                string assetFolder = Path.Combine(
                    SharedInfo.SkinsFolder.FullName,
                    game,
                    SharedInfo.InternalAssetsFolder,
                    guid
                );

                if (IsSkinned(origin, game))
                {
                    continue;
                }

                IOUtils.TryCreateDirectory(assetFolder, true);

                string target = Path.Combine(assetFolder, Path.GetFileName(origin));

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    DirectoryInfo targetFolder = Directory.CreateDirectory(target);
                    IOUtils.CopyFolder(new DirectoryInfo(origin), targetFolder);
                    IOUtils.CopyFile(origin+".meta", target+".meta");

                }
                else
                {
                    
                    IOUtils.CopyFile(origin, target);
                    IOUtils.CopyFile(origin+".meta", target+".meta");
                }
                GitUtils.Stage(assetFolder);
            }
            
            GitUtils.Untrack(assetPath, true);
        }
        
        public static bool IsSkinned(string assetPath) => ShapeShifterConfiguration.Instance.GameNames.Any(game => IsSkinned(assetPath, game));

        private static bool IsSkinned(string assetPath, string game)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            
            if (string.IsNullOrEmpty(guid))
                return false;
            
            string assetFolder = Path.Combine(
                SharedInfo.SkinsFolder.FullName,
                game, 
                SharedInfo.InternalAssetsFolder,
                guid
            );

            return Directory.Exists(assetFolder) && !IOUtils.IsFolderEmpty(assetFolder);
        }
        
        private static void OnDisable() {
            SharedInfo.DirtyAssets.Clear();
            PreviewPerAsset.Clear();
        }
        
        
        private static IEnumerable<string> GetEligibleAssetPaths(Object[] assets)
        {
            IEnumerable<string> assetPaths =
                assets.Select(AssetDatabase.GetAssetPath);
            RemoveEmptyAssetPaths(ref assetPaths);
            RemoveDuplicatedAssetPaths(ref assetPaths);
            RemoveAlreadySkinnedAssets(ref assetPaths);
            return assetPaths;
        }

        private static bool IsValidImageFormat(string extension) =>
            extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp";

        private static void RemoveEmptyAssetPaths(ref IEnumerable<string> assetPaths) =>
            assetPaths = assetPaths.Where(assetPath => !string.IsNullOrEmpty(assetPath));

        private static void RemoveDuplicatedAssetPaths(ref IEnumerable<string> assetPaths) =>
            assetPaths = assetPaths.Distinct();

        private static void RemoveAlreadySkinnedAssets(ref IEnumerable<string> assetPaths) =>
            assetPaths = assetPaths.Where(assetPath => !IsSkinned(assetPath));
    }
}