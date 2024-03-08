using GameEngine.Library.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
	[RequireComponent(typeof(BoardElement))]
	public class BoardProductionRegister : BoardElementRegister
	{
		[SerializeField] protected ProductionBuilding _productionBuilding;

		public override IPlacableData PlacableData => _productionBuilding;
	}
}