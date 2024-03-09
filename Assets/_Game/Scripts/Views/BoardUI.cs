using GameEngine.Library.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
    public class BoardUI : Singleton<BoardUI>
    {
        [SerializeField] private ProductionMenu _productionMenu;
        [SerializeField] private InformationMenu _informationMenu;

        public ProductionMenu ProductionMenu => _productionMenu;
        public InformationMenu InformationMenu => _informationMenu;

        public bool IsScreenPointOverMenu(Vector2 screenPoint)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(ProductionMenu.GetComponent<RectTransform>(), screenPoint) ||
				   RectTransformUtility.RectangleContainsScreenPoint(InformationMenu.GetComponent<RectTransform>(), screenPoint);
		}

		public void QuitApplication()
		{
			Application.Quit();
		}
	}
}