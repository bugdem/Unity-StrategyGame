using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace GameEngine.Game.Core 
{
    [CreateAssetMenu(fileName = "BoardSetting", menuName = "Game Engine/Board Setting")]
    public class BoardSetting : ScriptableObject
    {
        [SerializeField] private List<Production> _productions;

        private ReadOnlyCollection<Production> _readOnlyProductions;
		public ReadOnlyCollection<Production> Productions
        {
            get
            {
                if (_readOnlyProductions == null)
					_readOnlyProductions = _productions.AsReadOnly();

                return _readOnlyProductions;
            }
        }
	}
}