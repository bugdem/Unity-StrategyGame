using System;
using UnityEngine;

namespace GameEngine.Game.Core
{
	public interface IPlacableUnit : IPlacableData
	{
		public short Attack { get; }
		public float AttackSpeed { get; }
		public float MoveSpeed { get; }
		public RuntimeAnimatorController Controller { get; }
	}

    [CreateAssetMenu(fileName = "Unit", menuName = "Game Engine/Production Item/Unit")]
    public class ProductionItemUnit : ProductionItem, IPlacableUnit
	{
		[Header("Unit")]
		[SerializeField] private PlacableData _placable;
		[SerializeField] private short _attack = 2;
		[SerializeField] private float _attackSpeed = 1;
		[SerializeField] private float _moveSpeed = 1;
		[SerializeField] private RuntimeAnimatorController _runtimeAnimatorController;

		public PlacableData Placable => _placable;
		public short Attack => _attack;
		public float AttackSpeed => _attackSpeed;
		public RuntimeAnimatorController Controller => _runtimeAnimatorController;

		public short CurrentHealth => Placable.CurrentHealth;

		public short MaxHealth => Placable.MaxHealth;

		public float MoveSpeed => _moveSpeed;
	}
}