using GameEngine.Library.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
    public abstract class BoardElement : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer _icon;

		[Header("Placement")]
		[SerializeField, SortingLayer] protected int _placementDragLayer = 1;
		[SerializeField] protected int _placementDragSortingOrder = 1;

		public IPlacableData PlacableData { get; private set; }

		private int _defaultLayer;
		private int _defaultSortingOrder;

		public void SetPlacable(IPlacableData placableData)
		{
			PlacableData = placableData;

			_icon.sprite = placableData.Placable.BoardSprite;
		}

		public virtual void OnPlacementStarted()
		{
			_icon.sortingLayerID = _placementDragLayer;
			_icon.sortingOrder = _placementDragSortingOrder;
		}

		public virtual void OnPlacementEnded(bool isPlaced)
		{
			_icon.sortingLayerID = _defaultLayer;
			_icon.sortingOrder = _defaultSortingOrder;
		}

		public virtual void OnSelected()
		{

		}

		public virtual void OnDeSelected()
		{

		}

		private void OnEnable()
		{
			_defaultLayer = _icon.sortingLayerID;
			_defaultSortingOrder = _icon.sortingOrder;
		}

		private void OnDisable()
		{
			_icon.sortingLayerID = _defaultLayer;
			_icon.sortingOrder = _defaultSortingOrder;
		}
	}
}