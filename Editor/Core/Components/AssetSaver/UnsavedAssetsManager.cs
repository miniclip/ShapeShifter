using System;
using System.Collections.Generic;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;

namespace Miniclip.ShapeShifter.Saver
{
    public enum ModificationType { Modified, Deleted }

    [Serializable]
    public class ModifiedAssetInfo
    {
        public string assetPath;

        public ModificationType modificationType;

        public ModifiedAssetInfo(string assetPath, ModificationType modificationType)
        {
            this.assetPath = assetPath;
            this.modificationType = modificationType;
        }
    }

    [Serializable]
    public class ModifiedAssets
    {
        public List<ModifiedAssetInfo> Values = new List<ModifiedAssetInfo>();

        public bool ContainsAssetPath(string assetPath)
        {
            return Values.Any(modifiedAssetInfo => string.Equals(
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

        internal static void AddModifiedPath(string assetPathToAdd, ModificationType modificationType)
        {
            var currentModifiedAssets = GetCurrentModifiedAssetsFromEditorPrefs();

            if (currentModifiedAssets == null)
            {
                currentModifiedAssets = new ModifiedAssets();
            }

            var assetModificationToAdd = currentModifiedAssets.Values.FirstOrDefault(
                assetModification => assetModification.assetPath == assetPathToAdd
            );
            
            if (assetModificationToAdd != null)
            {
                return;
            }

            currentModifiedAssets.Values.Add(new ModifiedAssetInfo(assetPathToAdd, modificationType));

            StoreCurrentModifiedAssetsInEditorPrefs(currentModifiedAssets);
        }

        public static void RemovedModifiedPath(string assetPathToRemove)
        {
            var currentModifiedAssets = GetCurrentModifiedAssetsFromEditorPrefs();

            if (currentModifiedAssets == null)
            {
                currentModifiedAssets = new ModifiedAssets();
            }

            var assetModificationToRemove = currentModifiedAssets.Values.FirstOrDefault(
                assetModification => assetModification.assetPath == assetPathToRemove
            );
            
            if (assetModificationToRemove == null)
            {
                return;
            }
            
            currentModifiedAssets.Values.Remove(assetModificationToRemove);

            StoreCurrentModifiedAssetsInEditorPrefs(currentModifiedAssets);
        }

        internal static ModifiedAssets GetCurrentModifiedAssetsFromEditorPrefs()
        {
            var modifiedAssetsJson = Persistence.GetString(
                MODIFIED_ASSETS_PERSISTENCE_KEY,
                PersistenceType.MachinePersistent
            );

            if (string.IsNullOrEmpty(modifiedAssetsJson))
            {
                return null;
            }

            ModifiedAssets unsavedAssetsManager = new ModifiedAssets();
            EditorJsonUtility.FromJsonOverwrite(modifiedAssetsJson, unsavedAssetsManager);
            return unsavedAssetsManager;
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
        internal static void ClearModifiedAssetsList()
        {
            Persistence.SetString(
                MODIFIED_ASSETS_PERSISTENCE_KEY,
                string.Empty,
                PersistenceType.MachinePersistent
            );
        }
    }
}