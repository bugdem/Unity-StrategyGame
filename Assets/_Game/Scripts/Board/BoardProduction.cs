using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
    public class BoardProduction : BoardElement
    {
        [SerializeField] private Transform _spawnPoint;

		private bool _willShowSpawnPoint;

		public override void OnPlaced(BoardGrid boardGrid, Vector3Int bottomLeftCellIndex)
		{
			base.OnPlaced(boardGrid, bottomLeftCellIndex);

			// Position spawn point visual on the board after placed if the building has production items.
			if (PlacableData is IItemProducer itemProducer && itemProducer.ProductionItems.Count > 0)
			{
				Vector3Int targetSpawnCell = PlacedCellIndex + itemProducer.SpawnCellIndex;
				Vector3 spawnPoint = PlacedBoardGrid.Grid.GetCellCenterWorld(targetSpawnCell);

				_spawnPoint.position = spawnPoint;
				_willShowSpawnPoint = true;
			}
		}

		public override void OnSelected()
		{
			base.OnSelected();

			if (_willShowSpawnPoint)
				_spawnPoint.gameObject.SetActive(true);
		}

		public override void OnDeSelected()
		{
			base.OnDeSelected();

			_spawnPoint.gameObject.SetActive(false);
		}

		protected override void ResetValues()
		{
			base.ResetValues();

			_willShowSpawnPoint = false;
			_spawnPoint.gameObject.SetActive(false);
		}
	}
}