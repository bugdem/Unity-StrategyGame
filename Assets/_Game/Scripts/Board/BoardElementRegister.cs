using GameEngine.Library.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
	// Used for registering a board element to the board controller and pool manager.
	// This class is required on board element that is already placed on scene.
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
			if (!Application.isPlaying)
			{
				// Draw visuals	on scene view for the placable data.
				if (PlacableData != null)
				{
					var boardElement = GetComponent<BoardElement>();
					boardElement.SetPlacable(PlacableData, boardElement.FightingSide);
				}
			}
		}
	}
}