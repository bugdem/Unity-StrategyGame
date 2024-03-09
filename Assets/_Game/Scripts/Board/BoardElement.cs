using GameEngine.Library.Editor;
using GameEngine.Library.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
	[Serializable]
	public enum FightingSide : byte
	{
		Player,
		Enemy
	}

	// Represents a board element that can be placed on the board grid.
    public abstract class BoardElement : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer _icon;
		[SerializeField] protected SpriteOutline _spriteOutline;
		[SerializeField] protected FightingSide _fightingSide = FightingSide.Player;

		[Header("Placement")]
		[SerializeField, SortingLayer] protected int _placementDragLayer = 1;
		[SerializeField] protected int _placementDragSortingOrder = 1;

		public IPlacableData PlacableData { get; private set; }
		public Vector3Int PlacedCellIndex { get; private set; }
		public BoardGrid PlacedBoardGrid { get; private set; }
		public FightingSide FightingSide => _fightingSide;
		public int CurrentHealth { get; private set; }
		public int MaxHealth { get; private set; }
		public bool IsDestroyed => CurrentHealth <= 0;

		private int _defaultLayer;
		private int _defaultSortingOrder;

		public virtual void SetPlacable(IPlacableData placableData, FightingSide fightingSide)
		{
			PlacableData = placableData;

			_fightingSide = fightingSide;
			_icon.sprite = placableData.Placable.BoardSprite;
			_icon.transform.localPosition = placableData.Placable.BoardSpriteOffset;

			CurrentHealth = placableData.Placable.CurrentHealth;
			MaxHealth = placableData.Placable.MaxHealth;
		}

		public virtual void OnPlacementStarted()
		{
			_icon.sortingLayerID = _placementDragLayer;
			_icon.sortingOrder = _placementDragSortingOrder;
		}

		public virtual void OnPlacementFailed()
		{
			_icon.sortingLayerID = _defaultLayer;
			_icon.sortingOrder = _defaultSortingOrder;
		}

		public virtual void OnPlaced(BoardGrid boardGrid, Vector3Int bottomLeftCellIndex)
		{
			_icon.sortingLayerID = _defaultLayer;
			_icon.sortingOrder = _defaultSortingOrder;

			PlacedCellIndex = bottomLeftCellIndex;
			PlacedBoardGrid = boardGrid;
		}

		public virtual void OnSelected()
		{
			_spriteOutline.enabled = true;
			_spriteOutline.OutlineSize = 2;
			_spriteOutline.OutlineColor = Color.green;
		}

		public virtual void OnDeSelected()
		{
			_spriteOutline.enabled = false;
		}

		// Standing cell index may be different from PlacedCellIndex when element is moving on grid.
		public virtual Vector3Int GetStandingCellIndex()
		{
			if (PlacedBoardGrid == null)
				return PlacedCellIndex;

			return PlacedBoardGrid.GetBottomLeftCellIndex(transform.position, PlacableData.Placable.Size);
		}

		// Takes specified damage by attacker.
		// Returns true if destroyed after taking damage.
		public virtual bool TakeDamage(BoardElement attacker, int damage)
		{
			CurrentHealth -= damage;

			EventManager.TriggerEvent(new BoardElementEvent(this, BoardElementEventType.Damaged, damage, CurrentHealth.ClampMin(0)));

			if (CurrentHealth <= 0)
			{
				GetDestroyed();
				return true;
			}

			return false;
		}

		public virtual void GetDestroyed()
		{
			// Return to pool and notify listeners.
			PlacedBoardGrid.RemoveBoardElement(this);
			GetComponent<PoolableObject>().Destroy();

			EventManager.TriggerEvent(new BoardElementEvent(this, BoardElementEventType.Destroyed));
		}

		// Reset values for next pool use.
		protected virtual void ResetValues()
		{
			_icon.sortingLayerID = _defaultLayer;
			_icon.sortingOrder = _defaultSortingOrder;

			PlacedBoardGrid = null;
			PlacedCellIndex = Vector3Int.zero;
		}

		private void OnEnable()
		{
			_defaultLayer = _icon.sortingLayerID;
			_defaultSortingOrder = _icon.sortingOrder;
		}

		private void OnDisable()
		{
			ResetValues();
		}
	}
}