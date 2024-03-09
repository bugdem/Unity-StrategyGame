using DG.Tweening;
using GameEngine.Library.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.Game.Core
{
    public class InformationMenu : BoardMenu
    {
        [Header("Information - Selected Product")]
        [SerializeField] private TMPro.TextMeshProUGUI _productTitle;
        [SerializeField] private Image _productIcon;
		[SerializeField] private Transform _productionItemArea;
        [SerializeField] private Transform _productionItemContainer;

        [Header("Information - Window")]
        [SerializeField] private Ease _showEase = Ease.OutCubic;
        [SerializeField] private float _showDuration = 0.5f;

		// public bool IsShowing => BoardElementToShow != null;
		public bool IsShowing { get; private set;}
		public BoardElement BoardElementToShow { get; private set; }

		private Tween _showTween;
		private RectTransform _rectTransform;
		private List<InformationMenuProductItemView> _productItemViews = new();

		private void Awake()
		{
            _rectTransform = GetComponent<RectTransform>();

			ShowWindow(false, instant: true, force: true);
		}

		private void SetElementToShow(BoardElement boardElement)
		{
			// Clear previous element information.
			foreach (var productionItemView in _productItemViews)
				productionItemView.GetComponent<PoolableObject>().Destroy();

			_productItemViews.Clear();

			// Set new board element information.
			BoardElementToShow = boardElement;

			_productTitle.text = boardElement.PlacableData.Name;
			_productIcon.sprite = boardElement.PlacableData.Placable.BoardSprite;

			// If there are production items, show them.
			if (boardElement.PlacableData is IItemProducer itemProducer && itemProducer.ProductionItems.Count > 0)
			{
				_productionItemArea.gameObject.SetActive(true);

				// Feed production items to UI.
				foreach (var productionItem in itemProducer.ProductionItems)
				{
					// Create production item view.
					var poolableProductionItemView = PoolManager.Instance.GetMenuItemForProductionItem();
					poolableProductionItemView.transform.SetParent(_productionItemContainer);
					poolableProductionItemView.transform.localScale = Vector3.one;
					poolableProductionItemView.gameObject.SetActive(true);

					var productionItemView = poolableProductionItemView.GetComponent<InformationMenuProductItemView>();
					productionItemView.SetProductionItem(productionItem);

					_productItemViews.Add(productionItemView);
				}
			}
			else
				_productionItemArea.gameObject.SetActive(false);
		}

		public void Show(BoardElement boardElement, bool instant = false)
        {
			if (!IsShowing)
				ShowWindow(true, instant: instant);

			// Feed board element information to UI.
			SetElementToShow(boardElement);
		}

		public void Hide(bool instant = false)
		{
			ShowWindow(false, instant: instant);
		}

		private void ShowWindow(bool status, bool instant = false, bool force = false)
		{
			if (!force && IsShowing == status)
				return;

			IsShowing = status;

			if (_showTween != null)
			{
				_showTween.Kill();
				_showTween = null;
			}

			float targetPosX = 0f;

			if (status) gameObject.SetActive(true);
			else targetPosX = _rectTransform.rect.width;

			if (!instant)
			{
				_showTween = _rectTransform.DOAnchorPosX(targetPosX, _showDuration)
											.SetEase(_showEase)
											.OnComplete(() => 
											{
												gameObject.SetActive(status);
												_showTween = null;
											});
			}
			else
			{
				_rectTransform.anchoredPosition = new Vector2(targetPosX, _rectTransform.anchoredPosition.y);
				gameObject.SetActive(status);
			}
		}
	}
}