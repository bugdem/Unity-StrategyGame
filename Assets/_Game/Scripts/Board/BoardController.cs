using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEngine.Library.Utils;
using GameEngine.Game.Pathfinding;

namespace GameEngine.Game.Core
{
    public class BoardController : Singleton<BoardController>
    {
		[SerializeField] private BoardSetting _boardSetting;
		[SerializeField] private BoardGrid _boardGrid;
		public Texture2D _attackCursorTexture;

		public BoardSetting Setting => _boardSetting;
		public ProductionMenuItemView DraggingProductionMenuItemView { get; private set; }
		public BoardProduction DraggingBoardProduction { get; private set; }
		public BoardElement SelectedBoardElement { get; private set; }

		private BoardElementPlacement _boardElementPlacementInfo;
		private Vector3 _draggingBoardProductionLastCheckedPos = Vector3.zero;
		private const float _boardProductionPlaceCheckDistance = 0.01f;

		private Dictionary<Vector3Int, GridCellOverlay> _gridCellOverlays = new();

		private Pathfinder _pathfinder;

		protected override void Awake()
		{
			base.Awake();

			_boardElementPlacementInfo = new();
			_pathfinder = new Pathfinder(_boardGrid, PathfindNeighbourSearchType.AllDirection);
		}

		private void Update()
		{
			if (InputManager.TouchButton.IsTouchDown)
			{
				// Notify camera that it should start panning if touch is not over any menu.
				if (!BoardUI.Instance.IsScreenPointOverMenu(InputManager.TouchPosition))
					CameraController.Instance.StartPan();
			}
			else if (InputManager.TouchButton.IsTouchUp)
			{
				// If there is a dragging production, try to place it.
				if (DraggingProductionMenuItemView != null)
				{
					OnProductionMenuItemDeselected();
				}
				else
				{
					// There is no dragging production.
					CameraController.Instance.StopPan();
					
					bool deselectInformationMenu = true;
					if (BoardUI.Instance.IsScreenPointOverMenu(InputManager.TouchPosition))
					{
						deselectInformationMenu = false;
					}
					// If touch over board and it is a tap(like mouse click), check if there is a board element on the touch position.
					else if (InputManager.TouchButton.IsTap)
					{
						Vector3 touchWorldPosition = GetTouchWorldPosition();
						Vector3Int touchedCellPosition = _boardGrid.Grid.WorldToCell(touchWorldPosition);
						BoardElement selectedBoardElement = _boardGrid.GetBoardElement(touchedCellPosition);
						if (selectedBoardElement != null && selectedBoardElement.FightingSide == FightingSide.Player)
						{
							// There is a board element on the tap position.
							deselectInformationMenu = false;
							OnBoardElementSelected(selectedBoardElement);

							// Focus camera on the selected board element.
							CameraController.Instance.Focus(selectedBoardElement.transform.position);
						}
					}
					
					if (deselectInformationMenu && InputManager.TouchButton.IsTap)
					{
						// Tapped on empty space over board. Deselect information menu if it is showing.
						CameraController.Instance.StopFocus();

						if (SelectedBoardElement != null)
							OnBoardElementDeselected(SelectedBoardElement);
					}
				}
			}

			// If there is a dragging production, check if it can be placed.
			if (DraggingBoardProduction != null)
			{
				DraggingBoardProduction.transform.position = GetTouchWorldPosition();				
				if (Vector3.Distance(DraggingBoardProduction.transform.position, _draggingBoardProductionLastCheckedPos) > _boardProductionPlaceCheckDistance)
				{
					_draggingBoardProductionLastCheckedPos = DraggingBoardProduction.transform.position;

					Vector3Int bottomLeftCellIndex = _boardGrid.GetBottomLeftCellIndex(DraggingBoardProduction.transform.position, DraggingBoardProduction.PlacableData.Placable.Size);
					_boardElementPlacementInfo.Clear();
					_boardGrid.CheckBoardElementBounds(DraggingBoardProduction, DraggingBoardProduction.transform.position, _boardElementPlacementInfo
						,(currentCellIndex, status) =>
						{
							if (status)
								_gridCellOverlays[currentCellIndex - bottomLeftCellIndex].SetAvailableColor();
							else
								_gridCellOverlays[currentCellIndex - bottomLeftCellIndex].SetNotAvailableColor();
						});
				}

				foreach (var cellIndex in _boardElementPlacementInfo.AvailableCells)
					GEDebug.DrawCube(_boardGrid.Grid.GetCellCenterWorld(cellIndex), Color.green, _boardGrid.Grid.cellSize);

				foreach (var cellIndex in _boardElementPlacementInfo.NotAvailableCells)
					GEDebug.DrawCube(_boardGrid.Grid.GetCellCenterWorld(cellIndex), Color.red, _boardGrid.Grid.cellSize);
			}

			if (InputManager.MoveToPositionButton.IsTouchDown)
			{
				if (SelectedBoardElement != null 
					&& SelectedBoardElement.PlacableData is IPlacableUnit placableUnit 
					&& SelectedBoardElement is BoardProductionItem productionItem)
				{
					Vector3 touchWorldPosition = GetTouchWorldPosition();
					Vector3Int touchedCellIndex = _boardGrid.Grid.WorldToCell(touchWorldPosition);
					GEDebug.DrawCube(_boardGrid.Grid.GetCellCenterWorld(touchedCellIndex), Color.blue, _boardGrid.Grid.cellSize, 5f);

					if (_boardGrid.FindFirstEmptyCell(touchedCellIndex, productionItem, _boardElementPlacementInfo))
					{
						Vector3Int targetCellIndex = _boardElementPlacementInfo.BottomLeftCellIndex;
						GEDebug.DrawCube(_boardGrid.Grid.GetCellCenterWorld(targetCellIndex), Color.green, _boardGrid.Grid.cellSize, 5f);

						var path = _pathfinder.FindPath(SelectedBoardElement.GetStandingCellIndex(), targetCellIndex);
						if (path != null && path.Count > 0)
						{
							bool attack = false;
							var touchedBoardElement = _boardGrid.GetBoardElement(touchedCellIndex);
							if (touchedBoardElement != null && touchedBoardElement.FightingSide != SelectedBoardElement.FightingSide)
								attack = true;

							productionItem.MovePath(path, onPathCompleted: (reachedCellIndex) =>
							{
								// If an enemy is targeted, start attacking.
								if (attack)
									productionItem.StartAttacking(touchedBoardElement);
							});
							_boardGrid.MoveBoardElement(productionItem, path[path.Count - 1].GridIndex, false);
						}
					}
				}
			}

			{
				Vector3 touchWorldPosition = GetTouchWorldPosition();
				Vector3Int touchedCellPosition = _boardGrid.Grid.WorldToCell(touchWorldPosition);
				Vector3 cellCenter = _boardGrid.Grid.GetCellCenterWorld(touchedCellPosition);
				GEDebug.DrawCube(cellCenter, Color.red, _boardGrid.Grid.cellSize);
			}

			bool attackCursor = false;
			if (SelectedBoardElement != null && SelectedBoardElement.PlacableData is IPlacableUnit)
			{
				Vector3 touchWorldPosition = GetTouchWorldPosition();
				Vector3Int touchedCellIndex = _boardGrid.Grid.WorldToCell(touchWorldPosition);
				var touchedBoardElement = _boardGrid.GetBoardElement(touchedCellIndex);
				if (touchedBoardElement != null && touchedBoardElement.FightingSide != SelectedBoardElement.FightingSide)
				{
					attackCursor = true;
				}
			}

			if (attackCursor)
				Cursor.SetCursor(_attackCursorTexture, new Vector2(25,0), CursorMode.Auto);
			else
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}
		
		private void CreateGridCellOverlay(BoardProduction boardProduction)
		{
			ClearGridCellOverlay();

			_boardGrid.GetBottomLeftCellIndex(boardProduction.transform.position, boardProduction.PlacableData.Placable.Size);

			for (int x = 0; x < boardProduction.PlacableData.Placable.Size.x; x++)
			{
				for (int y = 0; y < boardProduction.PlacableData.Placable.Size.y; y++)
				{
					var gridCellIndex = new Vector3Int(x, y, 0);
					var gridCellOverlay = PoolManager.Instance.GetGridCellOverlay().GetComponent<GridCellOverlay>();
					gridCellOverlay.transform.SetParent(DraggingBoardProduction.transform);				
					
					gridCellOverlay.SetDefaultColor();
					gridCellOverlay.CellIndex = gridCellIndex;
					gridCellOverlay.transform.position = _boardGrid.GetWorldPositionFromLocalCellIndex(boardProduction, gridCellIndex);
					gridCellOverlay.transform.localScale = _boardGrid.Grid.cellSize;
					gridCellOverlay.gameObject.SetActive(true);

					_gridCellOverlays.Add(gridCellOverlay.CellIndex, gridCellOverlay);
				}
			}
		}

		private void ClearGridCellOverlay()
		{
			foreach (var gridCellOverlay in _gridCellOverlays.Values)
				gridCellOverlay.Destroy();

			_gridCellOverlays.Clear();
		}

		private BoardProduction CreateBoardProduction(IPlacableData placableData, Vector3 position)
		{
			var boardProduction = PoolManager.Instance.GetBoardProduction().GetComponent<BoardProduction>();
			SetBoardElement(boardProduction, placableData, position, FightingSide.Player);
			return boardProduction;
		}

		private BoardProductionItem CreateBoardProductionUnit(IPlacableUnit placableUnit, Vector3 position)
		{
			var boardProductionItem = PoolManager.Instance.GetBoardProductionItem().GetComponent<BoardProductionItem>();
			SetBoardElement(boardProductionItem, placableUnit, position, FightingSide.Player);

			return boardProductionItem;
		}

		private void SetBoardElement(BoardElement boardElement, IPlacableData placableData, Vector3 position, FightingSide fightingSide)
		{
			boardElement.transform.SetParent(transform);
			boardElement.transform.position = position;
			boardElement.gameObject.SetActive(true);
			boardElement.SetPlacable(placableData, fightingSide);
		}

		public void RegisterBoardElement(BoardElement boardElement, IPlacableData placableData, Vector3 position, FightingSide fightingSide)
		{
			SetBoardElement(boardElement, placableData, position, fightingSide);
			_boardElementPlacementInfo.Clear();
			_boardGrid.CheckBoardElementBounds(boardElement.PlacableData.Placable.Size, position, _boardElementPlacementInfo);
			_boardGrid.PlaceBoardElement(boardElement, _boardElementPlacementInfo);
		}

		private Vector3 GetTouchWorldPosition()
		{
			return CameraController.Instance.MainCamera.ScreenToWorldPoint(InputManager.TouchPosition);
		}

		public void OnProductionMenuItemSelected(ProductionMenuItemView productionMenuItemView)
		{
			// Production menu needs to be notified to make scrollview stop scrolling after item is selected.
			BoardUI.Instance.ProductionMenu.OnProductionMenuItemSelected(productionMenuItemView);
			DraggingProductionMenuItemView = productionMenuItemView;

			// Debug.Log("Pointer down: " + productionMenuItemView.name);

			// If selected production is placable, create board element for that production and set it as dragging object.
			if (productionMenuItemView.Production is IPlacableData placableData)
			{
				Vector3 newBoardProductionPosition = GetTouchWorldPosition();
				DraggingBoardProduction = CreateBoardProduction(placableData, newBoardProductionPosition);
				DraggingBoardProduction.OnPlacementStarted();

				CreateGridCellOverlay(DraggingBoardProduction);

				EventManager.TriggerEvent(new BoardElementPlacementEvent(DraggingBoardProduction, BoardElementPlacementStatus.Started));

				if (BoardUI.Instance.InformationMenu.IsShowing)
					OnBoardElementDeselected(BoardUI.Instance.InformationMenu.BoardElementToShow);
			}
		}

		public void OnProductionMenuItemDeselected()
		{
			// Notify Production menu to make scrollview scrollable again.
			BoardUI.Instance.ProductionMenu.OnProductionMenuItemDeselected();

			// Notify currently selected production menu item view that pointer up event has been triggered manually.
			DraggingProductionMenuItemView.OnPointerUp();

			if (DraggingBoardProduction != null)
			{
				// If dragging board production could not be placed, return it to the pool.
				if (BoardUI.Instance.IsScreenPointOverMenu(InputManager.TouchPosition)
					|| !_boardGrid.PlaceBoardElement(DraggingBoardProduction, _boardElementPlacementInfo))
				{
					EventManager.TriggerEvent(new BoardElementPlacementEvent(DraggingBoardProduction, BoardElementPlacementStatus.Failed));
					DraggingBoardProduction.GetComponent<PoolableObject>().Destroy();
				}
				else
					EventManager.TriggerEvent(new BoardElementPlacementEvent(DraggingBoardProduction, BoardElementPlacementStatus.Placed));

				ClearGridCellOverlay();
			}

			DraggingBoardProduction = null;
			DraggingProductionMenuItemView = null;

			// Debug.Log("Pointer up!");
		}

		public void OnInformationProductionItemSelected(InformationMenuProductItemView informationMenuItemView)
		{
			if (informationMenuItemView.ProductionItem is IPlacableUnit placableUnit)
			{
				BoardElement spawnerBoardElement = BoardUI.Instance.InformationMenu.BoardElementToShow;
				IItemProducer itemProducer = spawnerBoardElement.PlacableData as IItemProducer;
				Vector3Int targetSpawnCell = spawnerBoardElement.PlacedCellIndex + itemProducer.SpawnCellIndex;

				// Find available cells to spawn unit around spawn point.
				if (_boardGrid.FindFirstEmptyCell(targetSpawnCell, placableUnit.Placable.Size, _boardElementPlacementInfo))
				{
					var newUnit = CreateBoardProductionUnit(placableUnit, _boardElementPlacementInfo.Position);
					_boardGrid.PlaceBoardElement(newUnit, _boardElementPlacementInfo);

					EventManager.TriggerEvent(new BoardElementEvent(newUnit, BoardElementEventType.Spawned));
				}
			}
		}

		private void OnBoardElementSelected(BoardElement boardElement)
		{
			if (SelectedBoardElement != null)
				SelectedBoardElement.OnDeSelected();

			boardElement.OnSelected();
			SelectedBoardElement = boardElement;

			// Debug.Log("Selected: " + SelectedBoardElement.PlacableData.Name);

			if (boardElement.PlacableData is IItemProducer itemProducer)
				BoardUI.Instance.InformationMenu.Show(boardElement);
			else
				BoardUI.Instance.InformationMenu.Hide();

			EventManager.TriggerEvent(new BoardElementSelectEvent(boardElement, true));
		}

		private void OnBoardElementDeselected(BoardElement boardElement)
		{
			boardElement.OnDeSelected();
			BoardUI.Instance.InformationMenu.Hide();

			EventManager.TriggerEvent(new BoardElementSelectEvent(boardElement, false));

			SelectedBoardElement = null;
		}
	}

	#region Event Definitions
	public enum BoardElementPlacementStatus : byte
	{
		Started,
		Placed,
		Failed
	}

	public struct BoardElementPlacementEvent
	{
		public BoardElement BoardElement;
		public BoardElementPlacementStatus Status;

		public BoardElementPlacementEvent(BoardElement boardElement, BoardElementPlacementStatus status)
		{
			BoardElement = boardElement;
			Status = status;
		}
	}

	public struct BoardElementSelectEvent
	{
		public BoardElement BoardElement;
		public bool Status;

		public BoardElementSelectEvent(BoardElement boardElement, bool status)
		{
			BoardElement = boardElement;
			Status = status;
		}
	}

	public enum BoardElementEventType : byte
	{
		Spawned,
		Destroyed,
		Damaged
	}

	public struct BoardElementEvent
	{
		public BoardElement BoardElement;
		public BoardElementEventType EventType;
		public int Damage;
		public int RemainingHealth;

		public BoardElementEvent(BoardElement boardElement, BoardElementEventType eventType, int damage = 0, int remainingHealth = 0)
		{
			BoardElement = boardElement;
			EventType = eventType;
			Damage = damage;
			RemainingHealth = remainingHealth;
		}
	}
	#endregion
}