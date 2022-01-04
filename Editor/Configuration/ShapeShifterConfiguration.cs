using System.Collections.Generic;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    public class ShapeShifterConfiguration : ScriptableObject
    {
        //TODO: turn these lists into serializable HashSets 

        [SerializeField]
        private List<string> gameNames = new List<string>();
        internal List<string> GameNames
        {
            get => this.gameNames;
            set => this.gameNames = value;
        }

        [SerializeField]
        private List<string> skinnedExternalAssetPaths = new List<string>();
        public List<string> SkinnedExternalAssetPaths => this.skinnedExternalAssetPaths;

        [SerializeField]
        private List<string> modifiedAssetPaths = new List<string>();

        public List<string> ModifiedAssetPaths => modifiedAssetPaths;
    }
}