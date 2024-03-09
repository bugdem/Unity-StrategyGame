using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.Game.Core
{
    public class ProductionMenu : BoardMenu
    {
        [Header("Production - Items")]
        [SerializeField] private Transform _productsContainer;
		[SerializeField] private ScrollRect _scrollRect;
		[SerializeField] private InfiniteScrollView _infiniteScrollView;

        private List<ProductionMenuItemView> _productionMenuItemViews = new();

		private void Start()
		{
            CreateProductionItems();

			_infiniteScrollView.Initialize(_productionMenuItemViews, (index) => CreateProductionItem(index));
		}

		public void CreateProductionItems()
        {
            ClearProductionItems();

            for (int index = 0; index < BoardController.Instance.Setting.Productions.Count; index ++)
            {
				var newProductionMenuItem = CreateProductionItem(index);
				_productionMenuItemViews.Add(newProductionMenuItem);
			}
        }

		private ProductionMenuItemView CreateProductionItem(int productionIndex)
		{
			productionIndex = productionIndex % BoardController.Instance.Setting.Productions.Count;
			var product = BoardController.Instance.Setting.Productions[productionIndex];
			var poolableProductionMenuItem = PoolManager.Instance.GetMenuItemForProduction() as ProductionMenuItemView;
			poolableProductionMenuItem.transform.SetParent(_productsContainer);
			poolableProductionMenuItem.transform.localScale = Vector3.one;
			// poolableProductionMenuItem.transform.localPosition = Vector3.zero;
			// poolableProductionMenuItem.transform.localRotation = Quaternion.identity;
			poolableProductionMenuItem.gameObject.SetActive(true);

			poolableProductionMenuItem.SetProduct(product);

			return poolableProductionMenuItem;
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