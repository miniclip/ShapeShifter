using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Utils;
using Unity.EditorCoroutines.Editor;

// using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Miniclip.MUShapeShifter {
   
    public partial class ShapeShifter {
        private Vector2 scrollPosition;
        private bool showSkinner = true;
        private FileSystemWatcher watcher;

        private HashSet<string> dirtyAssets = new HashSet<string>();
        private Dictionary<string, Texture2D> previewPerAsset = new Dictionary<string, Texture2D>();
        
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

        private void OnAssetSkinnerEnable()
        {
            if (this.watcher == null)
            {
                this.watcher = new FileSystemWatcher();
                this.watcher.Path = this.skinsFolder.FullName;
                this.watcher.IncludeSubdirectories = true;
            }

            // To account for Unity's behaviour of sending consecutive OnEnables without an OnDisable
            this.watcher.Changed -= this.OnFileSystemChanged;
            this.watcher.Changed += this.OnFileSystemChanged;
            this.watcher.EnableRaisingEvents = true;
        }

        private bool IsSkinned(string assetPath)
        {
            foreach (var game in configuration.GameNames)
            {
                if (IsSkinned(assetPath, game))
                    return true;
            }

            return false;
        }
        
        private bool IsSkinned(string assetPath, string game)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            string assetFolder = Path.Combine(
                this.skinsFolder.FullName,
                game, 
                InternalAssetsFolder,
                guid
            );

            bool folderExists = Directory.Exists(assetFolder);
            bool isFolderEmpty = IOUtils.IsFolderEmpty(assetFolder);
            return folderExists && !isFolderEmpty;
        }
        
        private void DrawAssetSection(Object asset) {
            EditorGUILayout.InspectorTitlebar(true, asset);
            
            string path = AssetDatabase.GetAssetPath(asset);

            bool skinned = IsSkinned(path);

            Color oldColor = GUI.backgroundColor;
            
            if (skinned) {
                this.DrawSkinnedAssetSection(path);
            } else {
                this.DrawUnskinnedAssetSection(path);
            }
            
            GUI.backgroundColor = oldColor;
        }

        private void DrawAssetPreview(string key, string game, string path) {
            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            
            using (new GUILayout.VerticalScope(boxStyle)) {
                float buttonWidth = EditorGUIUtility.currentViewWidth * (1.0f / this.configuration.GameNames.Count);
                buttonWidth -= 20; // to account for a possible scrollbar or some extra padding 

                bool clicked = GUILayout.Button(
                    this.previewPerAsset[key],
                    GUILayout.Width(buttonWidth),
                    GUILayout.MaxHeight(buttonWidth)
                );
                
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

        private void DrawSkinnedAssetSection(string assetPath) {
            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            
            using (new GUILayout.HorizontalScope(boxStyle)) {
                foreach (string game in this.configuration.GameNames) {
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);
                    string skinnedPath = Path.Combine(
                        this.skinsFolder.FullName,
                        game,
                        InternalAssetsFolder,
                        guid,
                        Path.GetFileName(assetPath)
                    );

                    string key = this.GenerateAssetKey(game, guid);
                    this.GenerateAssetPreview(key, skinnedPath);
                    this.DrawAssetPreview(key, game, skinnedPath);
                }
            }

            GUI.backgroundColor = Color.red;
                
            if (GUILayout.Button("Remove skins")) {
                this.RemoveSkins(assetPath);
            }        
        }

        private void DrawUnskinnedAssetSection(string assetPath) {
            GUI.backgroundColor = Color.green;
                
            if (GUILayout.Button("Skin it!")) {
                this.SkinAsset(assetPath);
            }
        }

        private void GenerateAssetPreview(string key, string assetPath) {
            if (this.dirtyAssets.Contains(key) || ! this.previewPerAsset.ContainsKey(key)) {
                this.dirtyAssets.Remove(key);

                Texture2D texturePreview = EditorGUIUtility.FindTexture(errorIcon);
                if (Directory.Exists(assetPath)) {
                    texturePreview = EditorGUIUtility.FindTexture("Folder Icon");
                } else if (!File.Exists(assetPath)) {
                    texturePreview = EditorGUIUtility.FindTexture(errorIcon);
                } else {
                    string extension = Path.GetExtension(assetPath);

                    if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp") {
                        texturePreview = new Texture2D(0, 0);
                        texturePreview.LoadImage(File.ReadAllBytes(assetPath));
                    } else {
                        string icon = defaultIcon;

                        if (iconPerExtension.ContainsKey(extension)) {
                            icon = iconPerExtension[extension];
                        }

                        texturePreview = (Texture2D)EditorGUIUtility.IconContent(icon).image;
                    }
                }
                this.previewPerAsset[key] = texturePreview;
            }
        }
        
        
        private void OnAssetSkinnerGUI() {
            this.showSkinner = EditorGUILayout.Foldout(this.showSkinner, "Asset Skinner");

            if (! this.showSkinner) {
                return;
            }

            GUIStyle boxStyle = GUI.skin.GetStyle("Box");

            using (new GUILayout.VerticalScope(boxStyle)) {
                GUILayout.Label ("Selected assets:", EditorStyles.boldLabel);
                    
                Object[] assets = Selection.GetFiltered<Object>(SelectionMode.Assets);
                List<Object> supportedAssets = new List<Object>(assets.Length);
                
                foreach (Object asset in assets) {
                    Type assetType = asset.GetType();

                    if (assetType == typeof(DefaultAsset)) {
                        if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(asset))) {
                            supportedAssets.Add(asset);
                            continue;
                        }
                    }
                    
                    foreach (Type supportedType in SupportedTypes) {
                        if (assetType == supportedType || assetType.IsSubclassOf(supportedType)) {
                            supportedAssets.Add(asset);
                            break;
                        }
                    }
                }
                
                if (supportedAssets.Count == 0) {
                    GUILayout.Label("None.");
                } else {
                    this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition);
                        
                    foreach (Object asset in supportedAssets) {
                        this.DrawAssetSection(asset);
                    }
                
                    GUILayout.EndScrollView();
                }
            }
        }
        
        private void OnFileSystemChanged(object sender, FileSystemEventArgs args) {
            
            if (!configuration.UseFileSystemWatcher)
                return;
            
            //TODO: check if this opens a file handle INVESTIGATE
            DirectoryInfo assetDirectory = new DirectoryInfo(Path.GetDirectoryName(args.FullPath));
            string game = assetDirectory.Parent.Parent.Name;
            
            string key = this.GenerateAssetKey(game, assetDirectory.Name);
            this.dirtyAssets.Add(key);
        }

        private void OnSelectionChange() {
            this.dirtyAssets.Clear();
            this.previewPerAsset.Clear();
        }

        private void RemoveSkins(string assetPath) {
            foreach (string game in this.configuration.GameNames) {
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                string key = this.GenerateAssetKey(game, guid);
                this.dirtyAssets.Remove(key);
                this.previewPerAsset.Remove(key);
                
                string assetFolder = Path.Combine(
                    this.skinsFolder.FullName,
                    game,
                    InternalAssetsFolder,
                    guid
                );
                
                Directory.Delete(assetFolder, true);                
            }
            
        }

        private IEnumerator SkinMultipleAssetsEnumerator(Object[] assets, bool saveFirst = true)
        {
            EditorUtility.DisplayProgressBar("Skinning", "", 0);
            for (int index = 0; index < assets.Length; index++)
            {
                EditorUtility.DisplayProgressBar("Skinning", "", (float)index/assets.Length);
                Object asset = assets[index];
                string path = AssetDatabase.GetAssetPath(asset);
                yield return SkinAsset(path);
            }
            EditorUtility.DisplayProgressBar("Skinning", "", 1);
            EditorUtility.ClearProgressBar();
            yield return null;
        }

        private void SkinAssets(Object[] assets, bool saveFirst = true, bool useAsync = false)
        {
            if (useAsync)
            {
                EditorCoroutineUtility.StartCoroutine(SkinMultipleAssetsEnumerator(assets, saveFirst), this);
                return;
            }
            
            if (saveFirst)
            {
                this.SavePendingChanges();
            }

            List<string> paths = new List<String>();
            
            for (int index = 0; index < assets.Length; index++)
            {
                Object asset = assets[index];
                string path = AssetDatabase.GetAssetPath(asset);
               
                SkinAsset(path);
            }
        }
        
        private void SkinAssets(string[] assetPaths, bool saveFirst = true)
        {
            if (saveFirst)
            {
                this.SavePendingChanges();
            }
            
            foreach (string assetPath in assetPaths)
            {
                SkinAsset(assetPath, false);
            }
        }

        private IEnumerator SkinAsset(string assetPath, bool saveFirst = true)
        {
            if (saveFirst)
            {
                // make sure any pending changes are saved before generating copies
                this.SavePendingChanges();
            }

            foreach (string game in this.configuration.GameNames)
            {
                string origin = assetPath;
                string guid = AssetDatabase.AssetPathToGUID(origin);
                string assetFolder = Path.Combine(
                    this.skinsFolder.FullName,
                    game,
                    InternalAssetsFolder,
                    guid
                );

                if (IsSkinned(origin, game))
                {
                    Debug.Log($"SKIP {game} : {origin}");
                    continue;
                }
                else
                {
                    Debug.Log($"SKIN {game} : {origin}");
                }

                IOUtils.TryCreateDirectory(assetFolder);

                string target = Path.Combine(assetFolder, Path.GetFileName(origin));

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    DirectoryInfo targetFolder = Directory.CreateDirectory(target);
                    yield return IOUtils.CopyFolder(new DirectoryInfo(origin), targetFolder);
                }
                else
                {
                    yield return IOUtils.CopyFile(origin, target);
                }
            }

            yield return null;
        }
        
        private void OnDisable() {
            this.watcher.EnableRaisingEvents = false;
            this.watcher.Changed -= this.OnFileSystemChanged;
            
            this.dirtyAssets.Clear();
            this.previewPerAsset.Clear();
        }
    }
}