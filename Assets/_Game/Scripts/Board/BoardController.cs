using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEngine.Library.Utils;
using GameEngine.Game.Pathfinding;

namespace GameEngine.Game.Core
{
    public class BoardController : Singleton<BoardController>
    {
		[Header("Board")]
		[SerializeField] private BoardSetting _boardSetting;
		[SerializeField] private BoardGrid _boardGrid;

		[Header("Gameplay")]
		[SerializeField] private Texture2D _attackCursorTexture;
		[SerializeField] private Transform _movePointIndicator;

		public BoardSetting Setting => _boardSetting;
		public ProductionMenuItemView DraggingProductionMenuItemView { get; private set; }
		public BoardProduction DraggingBoardProduction { get; private set; }
		public BoardElement SelectedBoardElement { get; private set; }

		// Placement related variables.
		private BoardElementPlacement _boardElementPlacementInfo;
		private Vector3 _draggingBoardProductionLastCheckedPos = Vector3.zero;
		private const float _boardProductionPlaceCheckDistance = 0.01f;

		// Holds grid cell overlays for dragging board production.
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
					// If touch is over board and it is a tap(like mouse click), check if there is a board element on the touch position.
					else if (InputManager.TouchButton.IsTap)
					{
						Vector3 touchWorldPosition = GetTouchWorldPosition();
						Vector3Int touchedCellPosition = _boardGrid.Grid.WorldToCell(touchWorldPosition);
						BoardElement selectedBoardElement = _boardGrid.GetBoardElement(touchedCellPosition);

						// Check if there is a board element on touch and it is a player side element.
						if (selectedBoardElement != null && selectedBoardElement.FightingSide == FightingSide.Player)
						{
							// There is a board element on the tap position, select it.
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

				// If dragging production is moved, check if it can be placed on the new position.
				if (Vector3.Distance(DraggingBoardProduction.transform.position, _draggingBoardProductionLastCheckedPos) > _boardProductionPlaceCheckDistance)
				{
					_draggingBoardProductionLastCheckedPos = DraggingBoardProduction.transform.position;

					Vector3Int bottomLeftCellIndex = _boardGrid.GetBottomLeftCellIndex(DraggingBoardProduction.transform.position, DraggingBoardProduction.PlacableData.Placable.Size);
					_boardElementPlacementInfo.Clear();
					_boardGrid.CheckBoardElementBounds(DraggingBoardProduction, DraggingBoardProduction.transform.position, _boardElementPlacementInfo
						,(currentCellIndex, status) =>
						{
							// Set grid overlay colors according to availability of cells.
							if (status)
								_gridCellOverlays[currentCellIndex - bottomLeftCellIndex].SetAvailableColor();
							else
								_gridCellOverlays[currentCellIndex - bottomLeftCellIndex].SetNotAvailableColor();
						});
				}

#if UNITY_EDITOR
				foreach (var cellIndex in _boardElementPlacementInfo.AvailableCells)
					GEDebug.DrawCube(_boardGrid.Grid.GetCellCenterWorld(cellIndex), Color.green, _boardGrid.Grid.cellSize);

				foreach (var cellIndex in _boardElementPlacementInfo.NotAvailableCells)
					GEDebug.DrawCube(_boardGrid.Grid.GetCellCenterWorld(cellIndex), Color.red, _boardGrid.Grid.cellSize);
#endif
			}

			// Check if move position button like mouse right click is pressed.
			if (InputManager.MoveToPositionButton.IsTouchDown)
			{
				// Check if selected board element is a unit.
				if (SelectedBoardElement != null 
					&& SelectedBoardElement.PlacableData is IPlacableUnit placableUnit 
					&& SelectedBoardElement is BoardProductionItem productionItem)
				{
					// Find touched cell and check if it is available to move.
					Vector3 touchWorldPosition = GetTouchWorldPosition();
					Vector3Int touchedCellIndex = _boardGrid.Grid.WorldToCell(touchWorldPosition);
					GEDebug.DrawCube(_boardGrid.Grid.GetCellCenterWorld(touchedCellIndex), Color.blue, _boardGrid.Grid.cellSize, 5f);

					if (_boardGrid.FindFirstEmptyCell(touchedCellIndex, productionItem, _boardElementPlacementInfo))
					{
						Vector3Int targetCellIndex = _boardElementPlacementInfo.BottomLeftCellIndex;
						GEDebug.DrawCube(_boardGrid.Grid.GetCellCenterWorld(targetCellIndex), Color.green, _boardGrid.Grid.cellSize, 5f);

						// Create path to target position.
						var path = _pathfinder.FindPath(SelectedBoardElement.GetStandingCellIndex(), targetCellIndex);
						if (path != null && path.Count > 0)
						{
							// Check if an attackable enemy is targeted.
							bool attack = false;
							var touchedBoardElement = _boardGrid.GetBoardElement(touchedCellIndex);
							if (touchedBoardElement != null && touchedBoardElement.FightingSide != SelectedBoardElement.FightingSide)
								attack = true;

							_movePointIndicator.position = _boardGrid.Grid.GetCellCenterWorld(path[path.Count - 1].GridIndex);
							_movePointIndicator.gameObject.SetActive(true);

							// Move unit to target position.
							productionItem.MovePath(path, onPathCompleted: (reachedCellIndex) =>
							{
								// If an enemy is targeted, start attacking.
								if (attack)
									productionItem.StartAttacking(touchedBoardElement);
							});

							// Update grid info for moved unit.
							_boardGrid.MoveBoardElement(productionItem, path[path.Count - 1].GridIndex, false);
						}
					}
				}
			}

#if UNITY_EDITOR
			{
				// Draw touched cell for debugging.
				Vector3 touchWorldPosition = GetTouchWorldPosition();
				Vector3Int touchedCellPosition = _boardGrid.Grid.WorldToCell(touchWorldPosition);
				Vector3 cellCenter = _boardGrid.Grid.GetCellCenterWorld(touchedCellPosition);
				GEDebug.DrawCube(cellCenter, Color.red, _boardGrid.Grid.cellSize);
			}
#endif

			// Check if cursor should be changed to attack cursor.
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
		
		// Creates a grid overlay on dragging production item to show available and not available cells to place the production.
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

		// Called when a production in menu is deselected.
		public void OnProductionMenuItemDeselected()
		{
			// Notify Production menu to make scrollview scrollable again.
			BoardUI.Instance.ProductionMenu.OnProductionMenuItemDeselected(DraggingProductionMenuItemView);

			// Notify currently selected production menu item view that pointer up event has been triggered manually.
			DraggingProductionMenuItemView.OnPointerUp();

			if (DraggingBoardProduction != null)
			{
				// If dragging board production could not be placed on board, return it to the pool.
				if (BoardUI.Instance.IsScreenPointOverMenu(InputManager.TouchPosition)
					|| !_boardGrid.PlaceBoardElement(DraggingBoardProduction, _boardElementPlacementInfo))
				{
					EventManager.TriggerEvent(new BoardElementPlacementEvent(DraggingBoardProduction, BoardElementPlacementStatus.Failed));
					DraggingBoardProduction.GetComponent<PoolableObject>().Destroy();
				}
				// Dragging board production is placed on board, notify listerners.
				else
					EventManager.TriggerEvent(new BoardElementPlacementEvent(DraggingBoardProduction, BoardElementPlacementStatus.Placed));

				ClearGridCellOverlay();
			}

			DraggingBoardProduction = null;
			DraggingProductionMenuItemView = null;
		}

		// Called when a production item like a unit is selected from the information menu.
		public void OnInformationProductionItemSelected(InformationMenuProductItemView informationMenuItemView)
		{
			if (informationMenuItemView.ProductionItem is IPlacableUnit placableUnit)
			{
				// Find cell index to spawn unit.
				BoardElement spawnerBoardElement = BoardUI.Instance.InformationMenu.BoardElementToShow;
				IItemProducer itemProducer = spawnerBoardElement.PlacableData as IItemProducer;
				Vector3Int targetSpawnCell = spawnerBoardElement.PlacedCellIndex + itemProducer.SpawnCellIndex;

				// Find available cells to spawn unit around spawn point.
				if (_boardGrid.FindFirstEmptyCell(targetSpawnCell, placableUnit.Placable.Size, _boardElementPlacementInfo))
				{
					// Create unit and place it on board on first available cell.
					var newUnit = CreateBoardProductionUnit(placableUnit, _boardElementPlacementInfo.Position);
					_boardGrid.PlaceBoardElement(newUnit, _boardElementPlacementInfo);

					EventManager.TriggerEvent(new BoardElementEvent(newUnit, BoardElementEventType.Spawned));
				}
			}
		}

		// Called when an element on board is selected.
		private void OnBoardElementSelected(BoardElement boardElement)
		{
			if (SelectedBoardElement != null)
				SelectedBoardElement.OnDeSelected();

			boardElement.OnSelected();
			SelectedBoardElement = boardElement;

			if (boardElement.PlacableData is IItemProducer itemProducer)
				BoardUI.Instance.InformationMenu.Show(boardElement);
			else
				BoardUI.Instance.InformationMenu.Hide();

			EventManager.TriggerEvent(new BoardElementSelectEvent(boardElement, true));
		}

		// Called when an element on board is deselected.
		private void OnBoardElementDeselected(BoardElement boardElement)
		{
			_movePointIndicator.gameObject.SetActive(false);

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