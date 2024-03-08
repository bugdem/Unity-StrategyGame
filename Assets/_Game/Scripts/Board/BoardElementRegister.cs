using GameEngine.Library.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
	[RequireComponent(typeof(BoardElement))]
	public abstract class BoardElementRegister : RegisterWorldPoolableObject
	{
		public abstract IPlacableData PlacableData { get; }

		private BoardElement _boardElement;

		protected override void Awake()
		{
			base.Awake();

			_boardElement = GetComponent<BoardElement>();
		}

		protected virtual void Start()
		{
			BoardController.Instance.RegisterBoardElement(_boardElement, PlacableData, transform.position, _boardElement.FightingSide);
		}

		private void OnValidate()
		{
			if (PlacableData != null)
			{
				var boardElement = GetComponent<BoardElement>();
				boardElement.SetPlacable(PlacableData, boardElement.FightingSide);
			}
		}
	}
}