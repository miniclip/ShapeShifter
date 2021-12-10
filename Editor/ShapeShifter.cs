﻿using System;
using System.IO;
using System.Reflection;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Miniclip.MUShapeShifter {
    public partial class ShapeShifter : EditorWindow {
        private static readonly string ConfigurationResource = "ShapeShifterConfiguration.asset";
        private static readonly string ExternalAssetsFolder = "external";
        private static readonly string InternalAssetsFolder = "internal";
        
        private static readonly Type[] SupportedTypes = {
            typeof(AnimationClip),
            typeof(AnimatorController),
            typeof(DefaultAsset),
            typeof(GameObject),
            typeof(MonoScript),
            typeof(SceneAsset),
            typeof(ScriptableObject),
            typeof(Texture2D)
        };
        
        private ShapeShifterConfiguration configuration;
        private Editor defaultConfigurationEditor;
        private bool showConfiguration = false;
        
        private DirectoryInfo skinsFolder;

        [MenuItem("Window/Shape Shifter", false, (int)'G')]
        public static void ShowNextToInspector() {
            Assembly editorAssembly = typeof(Editor).Assembly;
            Type inspectorWindowType = editorAssembly.GetType("UnityEditor.InspectorWindow");
            
            GetWindow<ShapeShifter>(
                "Shape Shifter", 
                true, 
                inspectorWindowType
            );
        }

        private string GenerateAssetKey(string game, string guid) => game + ":" + guid;

        private void OnEnable() {
            this.InitialiseConfiguration();
            this.defaultConfigurationEditor = Editor.CreateEditor(
                this.configuration,
                typeof(ShapeShifterConfigurationEditor)
            );
            
            this.skinsFolder = new DirectoryInfo(Application.dataPath + "/../../Skins/");
            IOUtils.TryCreateDirectory(skinsFolder.FullName);
            
            this.OnAssetSkinnerEnable();
            this.OnExternalAssetSkinnerEnable();
            this.InitializeFileWatcher();
        }

        private FileSystemWatcher fileWatcher;
        private void InitializeFileWatcher()
        {
            if (this.fileWatcher == null) {
                Debug.Log($"Initializing FSW at {skinsFolder.FullName}");
                this.fileWatcher = new FileSystemWatcher();
                string skinsFolderFullName = this.skinsFolder.FullName+"FSWTEST/";
                IOUtils.TryCreateDirectory(skinsFolderFullName);
                this.fileWatcher.Path = skinsFolderFullName;
                this.fileWatcher.IncludeSubdirectories = true;
                this.fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            }
            fileWatcher.Dispose();
            // To account for Unity's behaviour of sending consecutive OnEnables without an OnDisable
            this.fileWatcher.Changed -= this.OnFileSystemChanged;
            this.fileWatcher.Changed += this.OnFileSystemChanged;
            this.fileWatcher.EnableRaisingEvents = true;
            
        }

        private void OnFileSystemChanged(object sender, FileSystemEventArgs args)
        {
            DirectoryInfo assetDirectory = new DirectoryInfo(Path.GetDirectoryName(args.FullPath));
            string game = assetDirectory.Parent.Parent.Name;
            string key = this.GenerateAssetKey(game, assetDirectory.Name);
            this.dirtyAssets.Add(key);
            Debug.Log($"FSW event: {args.Name}\n{args.ChangeType.ToString()}");
        }

        private void InitialiseConfiguration() {
            if (this.configuration != null) {
                return;
            }

            this.configuration = (ShapeShifterConfiguration)EditorGUIUtility.Load(
                ConfigurationResource
            );

            if (this.configuration == null)
            {
                this.configuration = CreateInstance<ShapeShifterConfiguration>();

                string folderPath = "Assets/Editor Default Resources/";

                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
                }

                AssetDatabase.CreateAsset(
                    this.configuration,
                    folderPath + ConfigurationResource
                );

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        
        private void OnGUI() {
            using (new GUILayout.VerticalScope()) {
                this.showConfiguration = EditorGUILayout.Foldout(this.showConfiguration, "Configuration");
                
                if (this.showConfiguration) {
                    this.defaultConfigurationEditor.OnInspectorGUI();
                    
                    // TODO: hide this when it's no longer necessary, as direct access to this list may cause issues
                    this.externalConfigurationEditor.OnInspectorGUI();
                }

                this.OnAssetSwitcherGUI();
                this.OnAssetSkinnerGUI();
                this.OnExternalAssetSkinnerGUI();

                if (GUILayout.Button("GC"))
                {
                    GC.Collect();
                }
                
                GUILayout.FlexibleSpace();
            }
            
            this.Repaint();
        }

        private void SavePendingChanges() {
            AssetDatabase.SaveAssets();
            
            // since the above doesn't seem to work with ScriptableObjects, might as well just go for a full save
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }
        
    }
}
