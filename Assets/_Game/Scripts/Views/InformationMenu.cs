using DG.Tweening;
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
        [SerializeField] private Transform _productionItemContainer;

        [Header("Information - Window")]
        [SerializeField] private Ease _showEase = Ease.OutCubic;
        [SerializeField] private float _showDuration = 0.5f;

        public bool IsShowing { get; private set; } = true;

        private RectTransform _rectTransform;
        private Tween _showTween;

		private void Awake()
		{
            _rectTransform = GetComponent<RectTransform>();

			Hide(instant: false);
		}

		public void Show(bool instant = false)
        {
            if (IsShowing)
			{

				return;
			}

			IsShowing = true;

			if (_showTween != null)
            {
				_showTween.Kill();
                _showTween = null;
			}

			gameObject.SetActive(true);

			if (!instant)
			{
                _showTween = _rectTransform.DOAnchorPosX(0f, _showDuration)
                                            .SetEase(_showEase)
                                            .OnComplete(() => { 
                                                                _showTween = null;
                                                            });
			}
            else
            {
                _rectTransform.anchoredPosition = new Vector2(0f, _rectTransform.anchoredPosition.y);
				gameObject.SetActive(true);
			}
		}

		public void Hide(bool instant = false)
		{
			if (!IsShowing)
				return;

			IsShowing = false;

			if (_showTween != null)
			{
				_showTween.Kill();
				_showTween = null;
			}

			if (!instant)
			{
				_showTween = _rectTransform.DOAnchorPosX(_rectTransform.rect.width, _showDuration)
											.SetEase(_showEase)
											.OnComplete(() => {
												gameObject.SetActive(false);
												_showTween = null;
											});
			}
			else
			{
				_rectTransform.anchoredPosition = new Vector2(_rectTransform.rect.width, _rectTransform.anchoredPosition.y);
				gameObject.SetActive(false);
			}
		}
	}
}