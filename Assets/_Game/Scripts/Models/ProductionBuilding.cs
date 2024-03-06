using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace GameEngine.Game.Core
{
    [CreateAssetMenu(fileName = "Building", menuName = "Game Engine/Production/Building")]
    public class ProductionBuilding : Production, IPlacableData
    {
        [Header("Building")]
        [SerializeField] private PlacableData _placable;
        [SerializeField] private List<ProductionItem> _productionItems;

		public PlacableData Placable => _placable;

		private ReadOnlyCollection<ProductionItem> _readOnlyProductionItems;
		public ReadOnlyCollection<ProductionItem> ProductionItems
        {
            get
            {
				if (_readOnlyProductionItems == null)
					_readOnlyProductionItems = _productionItems.AsReadOnly();

                return _readOnlyProductionItems;
			}
        }
	}
}