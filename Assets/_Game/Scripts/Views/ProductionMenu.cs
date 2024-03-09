using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.Game.Core
{
    public class ProductionMenu : BoardMenu
    {
        [Header("Production - Items")]
        [SerializeField] private Transform _productsContainer;
		[SerializeField] private ScrollRect _scrollRect;

        private List<ProductionMenuItemView> _productionMenuItemViews = new();

		private void Start()
		{
            CreateProductionItems();
		}

		public void CreateProductionItems()
        {
            ClearProductionItems();

            foreach (var product in BoardController.Instance.Setting.Productions)
            {
				var poolableProductItem = PoolManager.Instance.GetMenuItemForProduction();
				poolableProductItem.transform.SetParent(_productsContainer);
				poolableProductItem.transform.localScale = Vector3.one;
				poolableProductItem.transform.localPosition = Vector3.zero;
				poolableProductItem.transform.localRotation = Quaternion.identity;
				poolableProductItem.gameObject.SetActive(true);

				var poolableProductionMenuItem = poolableProductItem.GetComponent<ProductionMenuItemView>();

				poolableProductionMenuItem.SetProduct(product);
				_productionMenuItemViews.Add(poolableProductionMenuItem);
			}
        }

        private void ClearProductionItems()
        {
			foreach (var productMenuItemView in _productionMenuItemViews)
			{
				Destroy(productMenuItemView.gameObject);
			}
		}

		public void OnProductionMenuItemSelected(ProductionMenuItemView productionMenuItemView)
		{
			_scrollRect.StopMovement();
			_scrollRect.enabled = false;
		}

		public void OnProductionMenuItemDeselected(ProductionMenuItemView productionMenuItemView)
		{
			_scrollRect.enabled = true;
		}
	}
}