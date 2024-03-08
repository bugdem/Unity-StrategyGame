using GameEngine.Game.Pathfinding;
using GameEngine.Library.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
    public class BoardProductionItem : BoardElement
    {
		[Header("Production Item")]
        [SerializeField] private Animator _animator;
		

		protected Coroutine _moveCoroutine;
		protected Coroutine _attackCoroutine;
		protected string _animationEvent;

		public override void SetPlacable(IPlacableData placableData, FightingSide fightingSide)
		{
			base.SetPlacable(placableData, fightingSide);

			if (placableData is IPlacableUnit placableUnit)
				_animator.runtimeAnimatorController = placableUnit.Controller;
		}

		private void LookAt(Vector3Int targetCellIndex)
		{
			Vector2Int direction = (Vector2Int) (targetCellIndex - GetStandingCellIndex());
			float angle = GEMaths.AngleBetween(direction, Vector2.right);
			Vector2Int lookDirection = Vector2Int.zero;
			if ((angle < 22.5f) || (angle >= 342.5))
				lookDirection = new Vector2Int(1, 0);
			else if ((angle >= 22.5f) && (angle < 67.5f))
				lookDirection = new Vector2Int(1, 1);
			else if ((angle >= 67.5f) && (angle < 112.5f))
				lookDirection = new Vector2Int(0, 1);
			else if ((angle >= 112.5f) && (angle < 157.5f))
				lookDirection = new Vector2Int(-1, 1);
			else if ((angle >= 157.5f) && (angle < 202.5f))
				lookDirection = new Vector2Int(-1, 0);
			else if ((angle >= 202.5f) && (angle < 247.5f))
				lookDirection = new Vector2Int(-1, -1);
			else if ((angle >= 247.5f) && (angle < 292.5f))
				lookDirection = new Vector2Int(0, -1);
			else if ((angle >= 292.5f) && (angle < 337.5f))
				lookDirection = new Vector2Int(1, -1);

			_animator.SetFloat("Horizontal", lookDirection.x);
			_animator.SetFloat("Vertical", lookDirection.y);
		}

		public void MovePath(List<PathNode> pathNodes, Action<Vector3Int> onPathNodeReached = null, Action<Vector3Int> onPathCompleted = null)
		{
			if (pathNodes == null || pathNodes.Count == 0) return;

			StopMoving();
			StopAttacking();

			_moveCoroutine = StartCoroutine(MovePathCoroutine(pathNodes, onPathNodeReached, onPathCompleted));
		}

		private IEnumerator MovePathCoroutine(List<PathNode> pathNodes, Action<Vector3Int> onPathNodeReached = null, Action<Vector3Int> onPathCompleted = null)
		{
			IPlacableUnit placableUnit = PlacableData as IPlacableUnit;

			_animator.SetBool("Walk", true);
			_animator.Play("Walk");

			for (int i = 1; i < pathNodes.Count; i++)
			{
				var nextNode = pathNodes[i];
				var nextCell = PlacedBoardGrid.Grid.GetCellCenterWorld(nextNode.GridIndex);

				LookAt(nextNode.GridIndex);

				while (Vector3.Distance(transform.position, nextCell) > 0.01f)
				{
					transform.position = Vector3.MoveTowards(transform.position, nextCell, Time.deltaTime * placableUnit.MoveSpeed);
					yield return null;
				}

				onPathNodeReached?.Invoke(nextNode.GridIndex);
			}

			onPathCompleted?.Invoke(pathNodes[pathNodes.Count - 1].GridIndex);

			_animator.SetBool("Walk", false);
		}

		public virtual void OnAnimationEvent(string eventName)
		{
			_animationEvent = eventName;
		}

		public virtual void StartAttacking(BoardElement target)
		{
			StopAttacking();

			_attackCoroutine = StartCoroutine(IStartAttacking(target));
		}

		private IEnumerator IStartAttacking(BoardElement target)
		{
			IPlacableUnit thisUnit = PlacableData as IPlacableUnit;

			LookAt(target.PlacedCellIndex);

			while (!target.IsDestroyed)
			{
				_animator.Play("Attack");
				yield return new WaitUntil(() => _animationEvent != null && _animationEvent.Equals("Hit"));
				_animationEvent = null;

				if (!target.IsDestroyed)
					Attack(target);

				yield return new WaitForSeconds(1f / thisUnit.AttackSpeed);
			}
		}

		private void StopAttacking()
		{
			if (_attackCoroutine != null)
				StopCoroutine(_attackCoroutine);
		}

		private void StopMoving()
		{
			if (_moveCoroutine != null)
				StopCoroutine(_moveCoroutine);
		}

		public virtual void Attack(BoardElement target)
		{
			IPlacableUnit thisUnit = PlacableData as IPlacableUnit;
			target.TakeDamage(this, thisUnit.Attack);
		}
	}
}