using System.Collections;
using System.Collections.Generic;
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

		public bool IsHighlightEnabled => BoardController.Instance.SelectedProductionMenuItemView == null;
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

		public void OnPointerDown(PointerEventData eventData)
		{
			BoardController.Instance.OnProductionMenuItemSelected(this);
		}

		public void OnPointerUp()
		{

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