using GameEngine.Library.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
	[RequireComponent(typeof(BoardElement))]
    public class BoardProductionItemRegister : BoardElementRegister
	{
		[SerializeField] protected ProductionItemUnit _productionItemUnit;

		public override IPlacableData PlacableData => _productionItemUnit;
	}
}