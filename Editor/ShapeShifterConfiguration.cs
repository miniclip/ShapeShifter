using System.Collections.Generic;
using UnityEngine;

namespace NelsonRodrigues.ShapeShifter {
    public class ShapeShifterConfiguration : ScriptableObject {
        //TODO: turn these lists into serializable HashSets 
        
        [SerializeField]
        private List<string> gameNames;
        public List<string> GameNames => this.gameNames;

        [SerializeField]
        private List<string> skinnedExternalAssetPaths;
        public List<string> SkinnedExternalAssetPaths => this.skinnedExternalAssetPaths;
    }
}