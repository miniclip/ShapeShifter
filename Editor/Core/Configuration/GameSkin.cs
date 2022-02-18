using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    [Serializable]
    public class GameSkin
    {
        [SerializeField]
        private string externalSkinsFolderPath;
        public string ExternalSkinsFolderPath => externalSkinsFolderPath;

        [SerializeField]
        private string internalSkinsFolderPath;
        public string InternalSkinsFolderPath => internalSkinsFolderPath;

        [SerializeField]
        private string mainFolderPath;
        public string MainFolderPath => mainFolderPath;

        [SerializeField]
        public string Name;

        public static bool operator ==(GameSkin A, GameSkin B)
        {
            if (A is null)
            {
                if (B is null)
                {
                    return true;
                }

                return false;
            }

            return A.Equals(B);
        }

        public static bool operator !=(GameSkin A, GameSkin B) => !(A == B);

        public GameSkin(string name)
        {
            Debug.Log("##! Creating GameSkin Instance"); //TODO Optimize this

            SetUpGameSkin(name);
        }

        public bool HasAssetSkin(string guid) => GetAssetSkins().Any(assetSkin => assetSkin.Guid == guid);

        public AssetSkin GetAssetSkin(string guid)
        {
            List<AssetSkin> existing = GetAssetSkins();

            AssetSkin assetSkin = existing.FirstOrDefault(s => s.Guid == guid);

            return assetSkin;
        }

        public void Rename(string newName)
        {
            var newMainFolderPath = GetGameFolderPath(newName);
            FileUtil.MoveFileOrDirectory(mainFolderPath, newMainFolderPath);
            SetUpGameSkin(newName);
        }

        public bool Equals(GameSkin gameSkin)
        {
            if (gameSkin is null)
            {
                return false;
            }

            if (ReferenceEquals(this, gameSkin))
            {
                return true;
            }

            if (GetType() != gameSkin.GetType())
            {
                return false;
            }

            return Name == gameSkin.Name;
        }

        private void SetUpGameSkin(string name)
        {
            Name = name;

            SetUpFolderPaths(name);
        }

        private void SetUpFolderPaths(string name)
        {
            mainFolderPath = GetGameFolderPath(name);
            internalSkinsFolderPath = Path.Combine(mainFolderPath, ShapeShifterConstants.INTERNAL_ASSETS_FOLDER);
            externalSkinsFolderPath = Path.Combine(mainFolderPath, ShapeShifterConstants.EXTERNAL_ASSETS_FOLDER);
        }

        private string GetGameFolderPath(string name) => Path.Combine(ShapeShifter.SkinsFolder.FullName, name);

        internal List<AssetSkin> GetAssetSkins()
        {
            List<AssetSkin> assetSkins = new List<AssetSkin>();
            if (Directory.Exists(InternalSkinsFolderPath))
            {
                DirectoryInfo internalFolder = new DirectoryInfo(InternalSkinsFolderPath);
                foreach (DirectoryInfo directory in internalFolder.GetDirectories())
                {
                    assetSkins.Add(new AssetSkin(directory.Name, directory.FullName));
                }
            }

            return assetSkins;
        }

        internal bool HasValidFolders() => HasInternalSkins() || HasExternalSkins();

        internal bool HasExternalSkins() => DoesFolderExistAndHaveFiles(ExternalSkinsFolderPath);

        internal bool HasInternalSkins() => DoesFolderExistAndHaveFiles(InternalSkinsFolderPath);

        private bool DoesFolderExistAndHaveFiles(string path) =>
            Directory.Exists(path) && Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Any();

        internal void DeleteFolder()
        {
            if (Directory.Exists(mainFolderPath))
            {
                Directory.Delete(mainFolderPath, true);
            }
        }

        public void Duplicate(string newName)
        {
            GameSkin newSkin = new GameSkin(newName);
            if (Directory.Exists(newSkin.MainFolderPath))
            {
                Directory.Delete(newSkin.MainFolderPath, true);
            }

            FileUtil.CopyFileOrDirectory(mainFolderPath, newSkin.MainFolderPath);
        }
    }
}