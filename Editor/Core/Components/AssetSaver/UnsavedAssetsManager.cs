using System;
using System.Collections.Generic;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;

namespace Miniclip.ShapeShifter.Saver
{
    public enum ModificationType { Modified, Deleted }
    public enum SkinType { Internal, External }

    [Serializable]
    public class ModifiedAssetInfo
    {
        public string assetPath;
        public string description;
        public ModificationType modificationType;
        public SkinType skinType;

        public ModifiedAssetInfo(string assetPath, ModificationType modificationType, SkinType skinType)
        {
            this.assetPath = assetPath;
            this.modificationType = modificationType;
            this.skinType = skinType;
        }
    }

    [Serializable]
    public class ModifiedAssets
    {
        public List<ModifiedAssetInfo> Values = new List<ModifiedAssetInfo>();

        public bool ContainsAssetPath(string assetPath)
        {
            return Values.Any(
                modifiedAssetInfo => string.Equals(
                    modifiedAssetInfo.assetPath,
                    assetPath,
                    StringComparison.Ordinal
                )
            );
        }
    }

    [Serializable]
    public class UnsavedAssetsManager
    {
        private const string MODIFIED_ASSETS_PERSISTENCE_KEY = "SHAPESHIFTER_UNSAVED_MODIFIED_ASSETS";

        public static void RemoveByPath(string assetPathToRemove)
        {
            var currentModifiedAssets = GetCurrentModifiedAssetsFromEditorPrefs();

            var assetModificationToRemove = currentModifiedAssets?.Values.FirstOrDefault(
                assetModification => assetModification.assetPath == assetPathToRemove
            );

            if (assetModificationToRemove == null)
            {
                return;
            }

            currentModifiedAssets.Values.Remove(assetModificationToRemove);

            StoreCurrentModifiedAssetsInEditorPrefs(currentModifiedAssets);
        }

        public static bool HasUnsavedChanges()
        {
            return GetCurrentModifiedAssetsFromEditorPrefs().Values.Count > 0;
        }

        internal static void Add(ModifiedAssetInfo modifiedAssetInfo)
        {
            var currentModifiedAssets = GetCurrentModifiedAssetsFromEditorPrefs();

            if (currentModifiedAssets == null)
            {
                currentModifiedAssets = new ModifiedAssets();
            }

            var assetModificationToAdd = currentModifiedAssets.Values.FirstOrDefault(
                assetModification => assetModification.assetPath == modifiedAssetInfo.assetPath
            );

            if (assetModificationToAdd != null)
            {
                return;
            }

            currentModifiedAssets.Values.Add(modifiedAssetInfo);
            ShapeShifterLogger.Log($"Register unsaved changes in {modifiedAssetInfo.assetPath}.");

            StoreCurrentModifiedAssetsInEditorPrefs(currentModifiedAssets);
        }

        internal static ModifiedAssets GetCurrentModifiedAssetsFromEditorPrefs()
        {
            var modifiedAssetsJson = Persistence.GetString(
                MODIFIED_ASSETS_PERSISTENCE_KEY,
                PersistenceType.MachinePersistent
            );

            ModifiedAssets modifiedAssets = new ModifiedAssets();

            if (string.IsNullOrEmpty(modifiedAssetsJson))
            {
                return modifiedAssets;
            }

            EditorJsonUtility.FromJsonOverwrite(modifiedAssetsJson, modifiedAssets);
            return modifiedAssets;
        }

        private static void StoreCurrentModifiedAssetsInEditorPrefs(ModifiedAssets unsavedAssetsManager)
        {
            Persistence.SetString(
                MODIFIED_ASSETS_PERSISTENCE_KEY,
                EditorJsonUtility.ToJson(unsavedAssetsManager),
                PersistenceType.MachinePersistent
            );
        }

        [MenuItem("Window/Shape Shifter/Clear Modified Assets List")]
        internal static void ClearUnsavedChanges()
        {
            Persistence.SetString(
                MODIFIED_ASSETS_PERSISTENCE_KEY,
                string.Empty,
                PersistenceType.MachinePersistent
            );
        }
    }
}