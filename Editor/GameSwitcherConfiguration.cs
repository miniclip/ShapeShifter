using System.Collections.Generic;
using UnityEngine;

namespace NelsonRodrigues.GameSwitcher {
    public class GameSwitcherConfiguration : ScriptableObject {
        
        [SerializeField]
        private List<string> gameNames;
        public List<string> GameNames => this.gameNames;
    }
}