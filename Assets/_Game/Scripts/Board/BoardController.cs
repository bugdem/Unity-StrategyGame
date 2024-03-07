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

		private BoardElementPlacement _draggingBoardElementPlacementInfo;
		private Vector3 _draggingBoardProductionLastCheckedPos = Vector3.zero;
		private const float _boardProductionPlaceCheckDistance = 0.01f;

		protected override void Awake()
		{
			base.Awake();

			_draggingBoardElementPlacementInfo = new();
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

					// If touch over board and it is a tap(like mouse click), check if there is a board element on the touch position.
					bool deselectInformationMenu = true;
					if (!BoardUI.Instance.IsScreenPointOverMenu(InputManager.TouchPosition)
						&& InputManager.TouchButton.IsTap)
					{
						Vector3 touchWorldPosition = GetTouchWorldPosition();
						Vector3Int touchedCellPosition = _boardGrid.Grid.WorldToCell(touchWorldPosition);
						BoardElement selectedBoardElement = _boardGrid.GetBoardElement(touchedCellPosition);
						if (selectedBoardElement != null)
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

						if (BoardUI.Instance.InformationMenu.IsShowing)
							OnBoardElementDeselected(BoardUI.Instance.InformationMenu.BoardElementToShow);
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
					_draggingBoardElementPlacementInfo.Clear();
					_boardGrid.CheckBoardElementBounds(DraggingBoardProduction, DraggingBoardProduction.transform.position, _draggingBoardElementPlacementInfo);
				}

				foreach (var cellIndex in _draggingBoardElementPlacementInfo.AvailableCells)
					GEDebug.DrawCube(_boardGrid.Grid.GetCellCenterWorld(cellIndex), Color.green, _boardGrid.Grid.cellSize);

				foreach (var cellIndex in _draggingBoardElementPlacementInfo.NotAvailableCells)
					GEDebug.DrawCube(_boardGrid.Grid.GetCellCenterWorld(cellIndex), Color.red, _boardGrid.Grid.cellSize);
			}
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
			return CameraController.Instance.MainCamera.ScreenToWorldPoint(InputManager.TouchPosition);
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
					|| !_boardGrid.PlaceBoardElement(DraggingBoardProduction, _draggingBoardElementPlacementInfo))
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

		private void OnBoardElementSelected(BoardElement boardElement)
		{
			boardElement.OnSelected();

			BoardUI.Instance.InformationMenu.Show(boardElement);
		}

		private void OnBoardElementDeselected(BoardElement boardElement)
		{
			boardElement.OnDeSelected();

			BoardUI.Instance.InformationMenu.Hide();
		}
	}
}