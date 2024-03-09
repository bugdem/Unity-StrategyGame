using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameEngine.Game.Core
{
    public class ProductionMenuItemView : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMPro.TextMeshProUGUI _title;
        [SerializeField] private Image _icon;
		[SerializeField] private Transform _highlight;

		public bool IsHighlightEnabled => BoardController.Instance.DraggingProductionMenuItemView == null;
        public Production Production { get; private set; }

		public void SetProduct(Production product)
        {
			Production = product;

			_title.text = product.Name;
			_icon.sprite = product.MenuItemIcon;
		}

		private void Highlight(bool status)
		{
			_highlight.gameObject.SetActive(status);
		}

		// Handled by Unity's Event System.
		public void OnPointerDown(PointerEventData eventData)
		{
			Highlight(true);

			BoardController.Instance.OnProductionMenuItemSelected(this);
		}

		// Handled by in game events. Player may have released the mouse button outside of this view.
		// If so, we need to deselect this view manually.
		public void OnPointerUp()
		{
			Highlight(false);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (IsHighlightEnabled)
				Highlight(true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (IsHighlightEnabled)
				Highlight(false);
		}
	}
}