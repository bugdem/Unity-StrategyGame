using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEngine.Library.Utils;

namespace GameEngine.Game.Core
{
    public class BoardController : Singleton<BoardController>
    {
		[SerializeField] private BoardSetting _boardSetting;
		[SerializeField] private BoardGrid _boardGrid;

		public BoardSetting Setting => _boardSetting;
		public ProductionMenuItemView SelectedProductionMenuItemView { get; private set; }

		public Vector3 _position;

		private void Update()
		{
			if (InputManager.TouchButton.IsTouchDown)
			{
				Vector3 worldPos = Camera.main.ScreenToWorldPoint(InputManager.TouchPosition);
				_position = _boardGrid.Grid.GetCellCenterWorld(_boardGrid.Grid.WorldToCell(worldPos));
			}
			else if (InputManager.TouchButton.IsTouchUp)
			{
				if (SelectedProductionMenuItemView != null)
				{
					OnProductionMenuItemDeselected();
				}
			}

			if (SelectedProductionMenuItemView != null)
			{

			}

			GEDebug.DrawCube(_position, Color.red, Vector3.one * 0.5f);
		}

		public void OnProductionMenuItemSelected(ProductionMenuItemView productionMenuItemView)
		{
			// Production menu needs to be notified to make scrollview stop scrolling after item is selected.
			BoardUI.Instance.ProductionMenu.OnProductionMenuItemSelected(productionMenuItemView);
			SelectedProductionMenuItemView = productionMenuItemView;

			Debug.Log("Pointer down: " + productionMenuItemView.name);
		}

		public void OnProductionMenuItemDeselected()
		{
			// Notify Production menu to make scrollview scrollable again.
			BoardUI.Instance.ProductionMenu.OnProductionMenuItemDeselected();

			// Notify currently selected production menu item view that pointer up event has been triggered manually.
			SelectedProductionMenuItemView.OnPointerUp();

			PlaceProduction();

			SelectedProductionMenuItemView = null;

			Debug.Log("Pointer up!");
		}

		private bool PlaceProduction()
		{
			return true;
		}
	}
}