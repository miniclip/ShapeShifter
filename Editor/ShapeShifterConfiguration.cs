using System.Collections.Generic;
using UnityEngine;

namespace Miniclip.MUShapeShifter {
    public class ShapeShifterConfiguration : ScriptableObject {
        //TODO: turn these lists into serializable HashSets 
        
        [SerializeField]
        private List<string> gameNames;
        public List<string> GameNames => this.gameNames;

        [SerializeField]
        private List<string> skinnedExternalAssetPaths;
        public List<string> SkinnedExternalAssetPaths => this.skinnedExternalAssetPaths;

        [SerializeField]
        private List<Sprite> sprites;
        
        public List<Sprite> Sprites => sprites;
        
        [SerializeField]
        private bool useFileSystemWatcher;
        
        public bool UseFileSystemWatcher => useFileSystemWatcher;
        
    }
}