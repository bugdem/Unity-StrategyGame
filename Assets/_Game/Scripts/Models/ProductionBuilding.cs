using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace GameEngine.Game.Core
{
	public interface IItemProducer
	{
		Vector3Int SpawnCellIndex { get; }
		ReadOnlyCollection<ProductionItem> ProductionItems { get; }
	}

	[CreateAssetMenu(fileName = "Building", menuName = "Game Engine/Production/Building")]
    public class ProductionBuilding : Production, IPlacableData, IItemProducer
    {
        [Header("Building")]
        [SerializeField] private PlacableData _placable;
		[SerializeField] private Vector3Int _spawnCellIndex;
        [SerializeField] private List<ProductionItem> _productionItems;

		public PlacableData Placable => _placable;

		// Hide main data and expose it as read-only as we do not want SO to be modified at runtime.
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

		public Vector3Int SpawnCellIndex => _spawnCellIndex;
	}
}