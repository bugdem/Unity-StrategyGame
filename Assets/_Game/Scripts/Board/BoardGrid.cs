using GameEngine.Library.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
    [RequireComponent(typeof(Grid))]
    public class BoardGrid : MonoBehaviour
    {
        private Grid _grid;
        public Grid Grid
        {
            get
            {
                if (_grid == null)
					_grid = GetComponent<Grid>();

                return _grid;
            }
        }

		private Dictionary<Vector3, BoardElement> _boardElementDict = new();
		private HashSet<BoardElement> _boardElements = new();

		public bool CheckBoardElementBounds(BoardElement boardElement, Vector3 targetPosition, BoardElementPlacement placementInfo = null)
		{
			bool canBePlaced = true;
			Vector3 bottomLeftOffset = new Vector3(
													-Grid.cellSize.x * (boardElement.PlacableData.Placable.Size.x - 1) * .5f,
													-Grid.cellSize.y * (boardElement.PlacableData.Placable.Size.y - 1) * .5f,
													0f
												);
			Vector3 boardProductionBottomLeft = targetPosition + bottomLeftOffset;
			Vector3Int bottomLeftGridCellIndex = Grid.WorldToCell(boardProductionBottomLeft);
			for (int x = 0; x < boardElement.PlacableData.Placable.Size.x; x++)
			{
				for (int y = 0; y < boardElement.PlacableData.Placable.Size.y; y++)
				{
					Vector3Int cellIndex = bottomLeftGridCellIndex + new Vector3Int(x, y, 0);
					if (_boardElementDict.ContainsKey(cellIndex))
					{
						canBePlaced = false;
						placementInfo?.NotAvailableCells.Add(cellIndex);
					}
					else
					{
						placementInfo?.AvailableCells.Add(cellIndex);
					}
				}
			}

			if (placementInfo != null)
			{
				Vector3 boardProductionBottomLeftCellPosition = Grid.GetCellCenterWorld(bottomLeftGridCellIndex);
				placementInfo.Position = boardProductionBottomLeftCellPosition - bottomLeftOffset;
				placementInfo.CanBePlaced = canBePlaced;
			}

			return canBePlaced;
		}

		public bool PlaceBoardElement(BoardElement boardElement, Vector3 targetPosition)
		{
			var placementInfo = new BoardElementPlacement();
			if (!CheckBoardElementBounds(boardElement, targetPosition, placementInfo))
				return false;

			return PlaceBoardElement(boardElement, placementInfo);
		}

		public bool PlaceBoardElement(BoardElement boardElement, BoardElementPlacement placementInfo)
		{
			if (placementInfo == null || !placementInfo.CanBePlaced)
				return false;

			boardElement.transform.position = placementInfo.Position;
			foreach (var cellIndex in placementInfo.AvailableCells)
				_boardElementDict[cellIndex] = boardElement;

			_boardElements.Add(boardElement);

			return true;
		}
	}

	public class BoardElementPlacement
	{
		public bool CanBePlaced;
		public Vector3 Position;
		public HashSet<Vector3Int> AvailableCells = new();
		public HashSet<Vector3Int> NotAvailableCells = new();

		public void Clear()
		{
			Position = Vector3.zero;
			AvailableCells.Clear();
			NotAvailableCells.Clear();
		}
	}
}