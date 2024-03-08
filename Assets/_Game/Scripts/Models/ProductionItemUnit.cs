using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace GameEngine.Game.Core
{
	public interface IPlacableUnit : IPlacableData
	{
		public short Attack { get; }
		public float AttackSpeed { get; }
		public float MoveSpeed { get; }
		public AnimatorController Controller { get; }
	}

    [CreateAssetMenu(fileName = "Unit", menuName = "Game Engine/Production Item/Unit")]
    public class ProductionItemUnit : ProductionItem, IPlacableUnit
	{
		[Header("Unit")]
		[SerializeField] private PlacableData _placable;
		[SerializeField] private short _attack = 2;
		[SerializeField] private float _attackSpeed = 1;
		[SerializeField] private float _moveSpeed = 1;
		[SerializeField] private AnimatorController _animatorController;

		public PlacableData Placable => _placable;
		public short Attack => _attack;
		public float AttackSpeed => _attackSpeed;
		public AnimatorController Controller => _animatorController;

		public short CurrentHealth => Placable.CurrentHealth;

		public short MaxHealth => Placable.MaxHealth;

		public float MoveSpeed => _moveSpeed;
	}
}