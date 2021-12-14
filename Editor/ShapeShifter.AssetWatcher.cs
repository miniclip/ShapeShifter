namespace Miniclip.ShapeShifter
{
    public partial class ShapeShifter
    {
        public void RegisterModifiedAssetInUnity(string modifiedAssetPath)
        {
            if (configuration.ModifiedAssetPaths.Contains(modifiedAssetPath))
                return;
            
            if (!IsSkinned(modifiedAssetPath))
            {
                if (TryGetParentSkinnedFolder(modifiedAssetPath, out string skinnedFolderPath))
                {
                    RegisterModifiedAssetInUnity(skinnedFolderPath);
                }
                return;
            }
            
            configuration.ModifiedAssetPaths.Add(modifiedAssetPath);
        }

        private bool TryGetParentSkinnedFolder(string assetPath, out string skinnedParentFolderPath)
        {
            if (assetPath == "Assets/")
            {
                skinnedParentFolderPath = null;
                return false;
            }

            string[] parentFolders = assetPath.Split('/');

            for (int index = parentFolders.Length - 1; index >= 0; index--)
            {
                string parentFolder = string.Join("/", parentFolders, 0, index);

                if (IsSkinned(parentFolder))
                {
                    skinnedParentFolderPath = parentFolder;
                    return true;
                }
            }

            skinnedParentFolderPath = null;
            return false;
        }
    }
}