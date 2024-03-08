using GameEngine.Game.Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
	[RequireComponent(typeof(Grid))]
	public class BoardGrid : MonoBehaviour, IPathfindable
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

		public BoardElement GetBoardElement(Vector3Int cellIndex)
		{
			_boardElementDict.TryGetValue(cellIndex, out var boardElement);
			return boardElement;
		}

		private Vector3 GetOffsetToCenterFromBottomLeft(Vector2Int size)
		{
			return new Vector3(
								-Grid.cellSize.x * (size.x - 1) * .5f,
								-Grid.cellSize.y * (size.y - 1) * .5f,
								0f
							);
		}

		public Vector3Int GetBottomLeftCellIndex(Vector3 centerPosition, Vector2Int size)
		{
			Vector3 bottomLeftOffset = GetOffsetToCenterFromBottomLeft(size);
			Vector3 boardProductionBottomLeft = centerPosition + bottomLeftOffset;
			return Grid.WorldToCell(boardProductionBottomLeft);
		}

		public bool CheckBoardElementBounds(BoardElement boardElement, Vector3 centerPosition, BoardElementPlacement placementInfo = null)
		{
			return CheckBoardElementBounds(boardElement.PlacableData.Placable.Size, centerPosition, placementInfo);
		}

		public bool CheckBoardElementBounds(Vector2Int size, Vector3 centerPosition, BoardElementPlacement placementInfo = null)
		{
			Vector3Int bottomLeftGridCellIndex = GetBottomLeftCellIndex(centerPosition, size);
			return CheckBoardElementBounds(size, bottomLeftGridCellIndex, placementInfo);
		}

		public bool CheckBoardElementBounds(Vector2Int size, Vector3Int bottomLeftGridCellIndex, BoardElementPlacement placementInfo = null)
		{
			bool canBePlaced = CheckEmptyCells(size, bottomLeftGridCellIndex, false, (cellIndex, isEmpty) =>
			{
				if (!isEmpty)
					placementInfo?.NotAvailableCells.Add(cellIndex);
				else
					placementInfo?.AvailableCells.Add(cellIndex);
			});

			if (placementInfo != null)
			{
				Vector3 bottomLeftOffset = GetOffsetToCenterFromBottomLeft(size);
				Vector3 boardProductionBottomLeftCellPosition = Grid.GetCellCenterWorld(bottomLeftGridCellIndex);
				placementInfo.Position = boardProductionBottomLeftCellPosition - bottomLeftOffset;
				placementInfo.BottomLeftCellIndex = bottomLeftGridCellIndex;
				placementInfo.CanBePlaced = canBePlaced;
			}

			return canBePlaced;
		}

		private bool CheckEmptyCells(Vector2Int size, Vector3Int bottomLeftCellIndex, bool breakIfNotEmpty = true, System.Action<Vector3Int, bool> action = null)
		{
			bool isEmpty = true;
			for (int x = 0; x < size.x; x++)
			{
				for (int y = 0; y < size.y; y++)
				{
					Vector3Int cellIndex = bottomLeftCellIndex + new Vector3Int(x, y, 0);
					if (_boardElementDict.ContainsKey(cellIndex))
					{
						isEmpty = false;
						if (breakIfNotEmpty)
							return isEmpty;

						action?.Invoke(cellIndex, false);
					}
					else
					{
						action?.Invoke(cellIndex, true);
					}
				}
			}

			return isEmpty;
		}

		public bool PlaceBoardElement(BoardElement boardElement, Vector3Int targetCellIndex, bool modifyPosition = true)
		{
			var placementInfo = new BoardElementPlacement();
			CheckBoardElementBounds(boardElement.PlacableData.Placable.Size, targetCellIndex, placementInfo);
			return PlaceBoardElement(boardElement, placementInfo, modifyPosition);
		}

		public bool PlaceBoardElement(BoardElement boardElement, BoardElementPlacement placementInfo, bool modifyPosition = true)
		{
			if (placementInfo == null || !placementInfo.CanBePlaced)
			{
				boardElement.OnPlacementFailed();
				return false;
			}

			if (modifyPosition)
				boardElement.transform.position = placementInfo.Position;

			foreach (var cellIndex in placementInfo.AvailableCells)
				_boardElementDict[cellIndex] = boardElement;

			_boardElements.Add(boardElement);

			boardElement.OnPlaced(this, placementInfo.BottomLeftCellIndex);

			return true;
		}

		public bool FindFirstEmptyCell(Vector3Int targetCell, BoardElement boardElement, BoardElementPlacement placementInfo)
		{
			return FindFirstEmptyCell(targetCell, boardElement.PlacableData.Placable.Size, placementInfo);
		}

		public bool FindFirstEmptyCell(Vector3Int targetCell, Vector2Int size, BoardElementPlacement placementInfo)
		{
			placementInfo.Clear();
			if (CheckBoardElementBounds(size, targetCell, placementInfo))
				return true;

			return FindFirstEmptyCell(targetCell, size, 1, placementInfo);
		}

		// Finds first empty cell in range of target cell.
		// TODO(GE): This method should be optimized in future as complexity is currently high.
		private bool FindFirstEmptyCell(Vector3Int targetCell, Vector2Int size, int distance, BoardElementPlacement placementInfo)
		{
			for (int x = -distance; x <= distance; x++)
			{
				for (int y = -distance; y <= distance; y++)
				{
					if (x == 0 && y == 0)
						continue;

					Vector3Int bottomLeftGridCellIndex = targetCell + new Vector3Int(x, y, 0);
					placementInfo.Clear();
					if (CheckBoardElementBounds(size, bottomLeftGridCellIndex, placementInfo))
						return true;
				}
			}

			distance++;
			return FindFirstEmptyCell(targetCell, size, distance, placementInfo);
		}

		public void RemoveBoardElement(BoardElement boardElement)
		{
			_boardElements.Remove(boardElement);
			for (int x = 0; x < boardElement.PlacableData.Placable.Size.x; x++)
			{
				for (int y = 0; y < boardElement.PlacableData.Placable.Size.y; y++)
				{
					Vector3Int cellIndex = boardElement.PlacedCellIndex + new Vector3Int(x, y, 0);
					_boardElementDict.Remove(cellIndex);
				}
			}
		}

		public void MoveBoardElement(BoardElement boardElement, Vector3Int targetCell, bool modifyPosition = true)
		{
			RemoveBoardElement(boardElement);
			PlaceBoardElement(boardElement, targetCell, modifyPosition);
		}

		#region Pathfinding
		public bool CanBeMovedOver(Vector3 cellIndex)
		{
			return !_boardElementDict.ContainsKey(cellIndex);
		}

		public bool IsCellExist(Vector3 cellIndex)
		{
			// A grid boundary can be added here if needed.
			return true;
		}
		#endregion
	}

	public class BoardElementPlacement
	{
		public bool CanBePlaced;
		public Vector3 Position;
		public Vector3Int BottomLeftCellIndex;
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