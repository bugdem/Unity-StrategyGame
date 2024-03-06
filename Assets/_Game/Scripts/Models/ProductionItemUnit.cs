using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
    [CreateAssetMenu(fileName = "Unit", menuName = "Game Engine/Production Item/Unit")]
    public class ProductionItemUnit : ProductionItem, IPlacableData
    {
		[Header("Unit")]
		[SerializeField] private PlacableData _placable;
		[SerializeField] private int _attack = 2;

		public PlacableData Placable => _placable;
		public int Attack => _attack;
	}
}