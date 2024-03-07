using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEngine.Library.Utils;
using System;
using UnityEngine.UIElements;

namespace GameEngine.Game.Core
{
    public class BoardController : Singleton<BoardController>
    {
		[SerializeField] private BoardSetting _boardSetting;
		[SerializeField] private BoardGrid _boardGrid;

		public BoardSetting Setting => _boardSetting;
		public ProductionMenuItemView DraggingProductionMenuItemView { get; private set; }
		public BoardProduction DraggingBoardProduction { get; private set; }

		private Camera _camera;

		private BoardElementPlacement _draggingBoardElementPlacementInfo;
		private Vector3 _draggingBoardProductionLastCheckedPos = Vector3.zero;
		private const float _boardProductionPlaceCheckDistance = 0.01f;

		private void Start()
		{
			_camera = Camera.main;
			_draggingBoardElementPlacementInfo = new();
		}

		private void Update()
		{
			if (InputManager.TouchButton.IsTouchDown)
			{

			}
			else if (InputManager.TouchButton.IsTouchUp)
			{
				if (DraggingProductionMenuItemView != null)
				{
					OnProductionMenuItemDeselected();
				}
			}

			if (DraggingBoardProduction != null)
			{
				DraggingBoardProduction.transform.position = GetTouchWorldPosition();				
				if (Vector3.Distance(DraggingBoardProduction.transform.position, _draggingBoardProductionLastCheckedPos) > _boardProductionPlaceCheckDistance)
				{
					_draggingBoardProductionLastCheckedPos = DraggingBoardProduction.transform.position;
					_draggingBoardElementPlacementInfo.Clear();
					_boardGrid.CheckBoardElementBounds(DraggingBoardProduction, DraggingBoardProduction.transform.position, _draggingBoardElementPlacementInfo);
				}

				foreach (var cellIndex in _draggingBoardElementPlacementInfo.AvailableCells)
					GEDebug.DrawCube(_boardGrid.Grid.GetCellCenterWorld(cellIndex), Color.green, _boardGrid.Grid.cellSize);

				foreach (var cellIndex in _draggingBoardElementPlacementInfo.NotAvailableCells)
					GEDebug.DrawCube(_boardGrid.Grid.GetCellCenterWorld(cellIndex), Color.red, _boardGrid.Grid.cellSize);
			}
		}

		public void OnProductionMenuItemSelected(ProductionMenuItemView productionMenuItemView)
		{
			// Production menu needs to be notified to make scrollview stop scrolling after item is selected.
			BoardUI.Instance.ProductionMenu.OnProductionMenuItemSelected(productionMenuItemView);
			DraggingProductionMenuItemView = productionMenuItemView;

			Debug.Log("Pointer down: " + productionMenuItemView.name);

			// If selected production is placable, create board element for that production and set it as dragging object.
			if (productionMenuItemView.Production is IPlacableData placableData)
			{
				Vector3 newBoardProductionPosition = GetTouchWorldPosition();
				DraggingBoardProduction = CreateBoardProduction(placableData, newBoardProductionPosition);
				DraggingBoardProduction.OnPlacementStarted();
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
				if (!_boardGrid.PlaceBoardElement(DraggingBoardProduction, _draggingBoardElementPlacementInfo))
				{
					DraggingBoardProduction.OnPlacementEnded(false);
					DraggingBoardProduction.GetComponent<PoolableObject>().Destroy();
				}
				else
					DraggingBoardProduction.OnPlacementEnded(true);
			}

			DraggingBoardProduction = null;
			DraggingProductionMenuItemView = null;

			Debug.Log("Pointer up!");
		}

		private BoardProduction CreateBoardProduction(IPlacableData placableData, Vector3 position)
		{
			var boardProductionPoolable = PoolManager.Instance.GetBoardProduction();
			boardProductionPoolable.transform.position = position;
			boardProductionPoolable.gameObject.SetActive(true);

			var boardProduction = boardProductionPoolable.GetComponent<BoardProduction>();
			boardProduction.SetPlacable(placableData);

			return boardProduction;
		}

		private Vector3 GetTouchWorldPosition()
		{
			return _camera.ScreenToWorldPoint(InputManager.TouchPosition);
		}
	}
}