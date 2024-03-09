using GameEngine.Library.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.Game.Core
{
    public class InfiniteScrollView : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _viewportTransform;
        [SerializeField] private RectTransform _contentTransform;
        [SerializeField] private GridLayoutGroup _gridLayoutGroup;

        private float _cellHeight;
        private bool _isInitialized;
        private int _itemsCount;
        private int _verticalItemCount;
        private bool _isUpdated;
        private Vector2 _oldVelocity;

        public void Initialize<T>(List<T> items, Func<int, T> createItem) where T : PoolableObject
        {
            _cellHeight = _gridLayoutGroup.cellSize.y + _gridLayoutGroup.spacing.y;
            _itemsCount = items.Count;
            _verticalItemCount = Mathf.CeilToInt(items.Count / (float)_gridLayoutGroup.constraintCount);
            _isUpdated = false;
            _oldVelocity = Vector2.zero;

            int itemsToAdd = Mathf.CeilToInt(_viewportTransform.rect.height / _cellHeight) * _gridLayoutGroup.constraintCount;

            // Add items to the bottom of the scroll view.
            for (int index = 0; index < itemsToAdd; index++)
            {
                RectTransform newItem = createItem(index).GetComponent<RectTransform>();
                newItem.SetParent(_contentTransform);
                newItem.SetAsLastSibling();
            }

            // Add items to the top of the scroll view.
			for (int index = 0; index < itemsToAdd; index++)
			{
                // Reverse the index to get the last item.
                int nextIndex = items.Count - index - 1;
                while (nextIndex < 0)
                {
					nextIndex += items.Count;
				}

				RectTransform newItem = createItem(nextIndex).GetComponent<RectTransform>();
				newItem.SetParent(_contentTransform);
				newItem.SetAsFirstSibling();
			}

            // Focus to the first item as top most item in scroll view.
			Canvas.ForceUpdateCanvases();

			Vector3 targetAnchor =
					(Vector2)_scrollRect.transform.InverseTransformPoint(_contentTransform.position)
					- (Vector2)_scrollRect.transform.InverseTransformPoint(items[0].transform.position);

            targetAnchor += new Vector3(0f, (_viewportTransform.rect.height - _cellHeight) * .5f); 
            targetAnchor.x = _contentTransform.anchoredPosition.x;

			_contentTransform.anchoredPosition = targetAnchor;

			_isInitialized = true;
		}

		private void Update()
		{
            if (!_isInitialized) return;

            if (_isUpdated)
            {
                _isUpdated = false;
                _scrollRect.velocity = _oldVelocity;
            }

            if (_contentTransform.anchoredPosition.y > 0f)
            {
                Canvas.ForceUpdateCanvases();
                _oldVelocity = _scrollRect.velocity;
                _contentTransform.anchoredPosition -= new Vector2(0f, _verticalItemCount * _cellHeight);
                _isUpdated = true;
            }

            if (_contentTransform.anchoredPosition.y < (0f - _cellHeight * _verticalItemCount))
            {
				Canvas.ForceUpdateCanvases();
                _oldVelocity = _scrollRect.velocity;
				_contentTransform.anchoredPosition += new Vector2(0f, _verticalItemCount * _cellHeight);
                _isUpdated = true;
			}
		}
	}
}