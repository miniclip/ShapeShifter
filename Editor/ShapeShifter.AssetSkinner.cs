using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace NelsonRodrigues.ShapeShifter {
   
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

        private void DrawAssetSection(Object asset) {
            EditorGUILayout.InspectorTitlebar(true, asset);
            
            string path = AssetDatabase.GetAssetPath(asset);
            Debug.Log(path);
            AssetImporter importer = AssetImporter.GetAtPath(path);
            bool skinned = importer.userData.Contains(ShapeShifter.SkinnedUserData);

            Color oldColor = GUI.backgroundColor;
            
            if (skinned) {
                this.DrawSkinnedAssetSection(importer);
            } else {
                this.DrawUnskinnedAssetSection(importer);
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

        private void DrawSkinnedAssetSection(AssetImporter importer) {
            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            
            using (new GUILayout.HorizontalScope(boxStyle)) {
                foreach (string game in this.configuration.GameNames) {
                    string guid = AssetDatabase.AssetPathToGUID(importer.assetPath);
                    string assetPath = Path.Combine(
                        this.skinsFolder.FullName,
                        game,
                        ShapeShifter.InternalAssetsFolder,
                        guid,
                        Path.GetFileName(importer.assetPath)
                    );

                    string key = this.GenerateAssetKey(game, guid);
                    this.GenerateAssetPreview(key, assetPath);
                    this.DrawAssetPreview(key, game, assetPath);
                }
            }

            GUI.backgroundColor = Color.red;
                
            if (GUILayout.Button("Remove skins")) {
                this.RemoveSkins(importer);
            }        
        }

        private void DrawUnskinnedAssetSection(AssetImporter importer) {
            GUI.backgroundColor = Color.green;
                
            if (GUILayout.Button("Skin it!")) {
                this.SkinAsset(importer);
            }
        }

        private void GenerateAssetPreview(string key, string assetPath) {
            if (this.dirtyAssets.Contains(key) || ! this.previewPerAsset.ContainsKey(key)) {
                this.dirtyAssets.Remove(key);

                Texture2D texturePreview;
                
                if (!File.Exists(assetPath)) {
                    texturePreview = EditorGUIUtility.FindTexture(ShapeShifter.errorIcon);
                } else {
                    string extension = Path.GetExtension(assetPath);

                    if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp") {
                        texturePreview = new Texture2D(0, 0);
                        texturePreview.LoadImage(File.ReadAllBytes(assetPath));
                    } else {
                        string icon = ShapeShifter.defaultIcon;

                        if (ShapeShifter.iconPerExtension.ContainsKey(extension)) {
                            icon = ShapeShifter.iconPerExtension[extension];
                        }

                        texturePreview = (Texture2D)EditorGUIUtility.IconContent(icon).image;
                    }
                }

                this.previewPerAsset[key] = texturePreview;
            }
        }
        
        private void OnAssetSkinnerEnable() {
            if (this.watcher == null) {
                this.watcher = new FileSystemWatcher();
                this.watcher.Path = this.skinsFolder.FullName;
                this.watcher.IncludeSubdirectories = true;
            }
            
            // To account for Unity's behaviour of sending consecutive OnEnables without an OnDisable
            this.watcher.Changed -= this.OnFileSystemChanged;
            this.watcher.Changed += this.OnFileSystemChanged;
            this.watcher.EnableRaisingEvents = true;
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
                    
                    foreach (Type supportedType in ShapeShifter.SupportedTypes) {
                        if (assetType == supportedType || assetType.IsSubclassOf(supportedType)) {
                            supportedAssets.Add(asset);
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
        
        private void OnDisable() {
            this.watcher.EnableRaisingEvents = false;
            this.watcher.Changed -= this.OnFileSystemChanged;
            
            this.dirtyAssets.Clear();
            this.previewPerAsset.Clear();
        }

        private void OnFileSystemChanged(object sender, FileSystemEventArgs args) {
            DirectoryInfo assetDirectory = new DirectoryInfo(Path.GetDirectoryName(args.FullPath));
            string game = assetDirectory.Parent.Parent.Name;
            
            string key = this.GenerateAssetKey(game, assetDirectory.Name);
            this.dirtyAssets.Add(key);
        }

        private void OnSelectionChange() {
            this.dirtyAssets.Clear();
            this.previewPerAsset.Clear();
        }

        private void RemoveSkins(AssetImporter importer) {
            foreach (string game in this.configuration.GameNames) {
                string guid = AssetDatabase.AssetPathToGUID(importer.assetPath);
                string key = this.GenerateAssetKey(game, guid);
                this.dirtyAssets.Remove(key);
                this.previewPerAsset.Remove(key);
                
                string assetFolder = Path.Combine(
                    this.skinsFolder.FullName,
                    game,
                    ShapeShifter.InternalAssetsFolder,
                    guid
                );
                
                Directory.Delete(assetFolder, true);                
            }

            importer.userData = importer.userData.Replace(
                ShapeShifter.SkinnedUserData,
                string.Empty
            );
            importer.SaveAndReimport();
        }

        private void SkinAsset(AssetImporter importer) {
            // make sure any pending changes are saved before generating copies
            this.SavePendingChanges();
            
            foreach (string game in this.configuration.GameNames) {
                string origin = importer.assetPath;
                string guid = AssetDatabase.AssetPathToGUID(origin);
                string assetFolder = Path.Combine(
                    this.skinsFolder.FullName,
                    game, 
                    ShapeShifter.InternalAssetsFolder,
                    guid
                );

                if (!Directory.Exists(assetFolder)) {
                    Directory.CreateDirectory(assetFolder);
                }

                string target = Path.Combine(assetFolder, Path.GetFileName(origin));
                File.Copy(origin, target, true);
            }
                    
            importer.userData += ShapeShifter.SkinnedUserData;
            importer.SaveAndReimport();
        }
    }
}