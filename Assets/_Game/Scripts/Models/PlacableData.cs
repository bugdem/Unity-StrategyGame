using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace GameEngine.Game.Core
{
	public interface IPlacableData
	{
		PlacableData Placable { get; }
		public string Name { get; }
	}

	[Serializable]
	public class PlacableData
	{
		[SerializeField] private Sprite _boardSprite;
		[SerializeField] private Vector2Int _size;
		[SerializeField] private short _currentHealth;
		[SerializeField] private short _maxHealth;

		public Sprite BoardSprite => _boardSprite;
		public Vector2Int Size => _size;
		public short CurrentHealth => _currentHealth;
		public short MaxHealth => _maxHealth;
	}
}