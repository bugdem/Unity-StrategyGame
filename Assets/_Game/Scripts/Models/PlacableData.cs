using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
	public interface IPlacableData
	{
		PlacableData Placable { get; }
	}

	[Serializable]
	public class PlacableData
	{
		[SerializeField] private Sprite _boardSprite;
		[SerializeField] private Vector2Int _size;
		[SerializeField] private int _currentHealth;
		[SerializeField] private int _maxHealth;

		public Sprite BoardSprite => _boardSprite;
		public Vector2Int Size => _size;
		public int CurrentHealth => _currentHealth;
		public int MaxHealth => _maxHealth;
	}
}