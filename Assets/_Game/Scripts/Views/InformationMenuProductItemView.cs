using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameEngine.Game.Core
{
    public class InformationMenuProductItemView : MonoBehaviour, IPointerDownHandler
	{
        [SerializeField] private Image _icon;
        [SerializeField] private TMPro.TextMeshProUGUI _name;

        public ProductionItem ProductionItem { get; private set; }

        public void SetProductionItem(ProductionItem productionItem)
        {
            ProductionItem = productionItem;

			_icon.sprite = productionItem.MenuItemIcon;
			_name.text = productionItem.Name;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			BoardController.Instance.OnInformationProductionItemSelected(this);
		}
	}
}