using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Pathfinding
{
	public enum PathfindNeighbourSearchType
	{
		PlusShaped,
		AllDirection
	}

	public interface IPathfindable
	{
		bool CanBeMovedOver(Vector3 cellIndex);
		bool IsCellExist(Vector3 cellIndex);
	}

	public class Pathfinder
    {
		// A* Pathfind variables
		private const int MOVE_STRAIGHT_COST = 10;
		private const int MOVE_DIAGONAL_COST = 14;

		private IPathfindable _pathfindable;
		private PathfindNeighbourSearchType _neighbourSearchType;
		private List<PathNode> _pathfindOpenList = new();
		private HashSet<PathNode> _pathfindClosedList = new();
		private Dictionary<Vector3Int, PathNode> _pathfindTempNeighbours = new();
		private Dictionary<Vector3Int, PathNode> _tempAllNodes = new();

		public Pathfinder(IPathfindable pathfindable, PathfindNeighbourSearchType neighbourSearchType)
		{
			_pathfindable = pathfindable;
			_neighbourSearchType = neighbourSearchType;
		}

		public List<PathNode> FindPath(Vector3Int startGridIndex, Vector3Int endGridIndex)
		{
			ClearCache();

			var startNode = GetPathNode(startGridIndex, true);
			var endNode = GetPathNode(endGridIndex, true);

			if (startNode == null || endNode == null) return null;

			_pathfindOpenList.Add(startNode);

			startNode.GCost = 0;
			startNode.HCost = CalculateDistanceCost(startNode, endNode);
			startNode.CalculateFCost();

			int loopCount = 0;
			while (_pathfindOpenList.Count > 0)
			{
				var currentNode = GetLowestFCostNode(_pathfindOpenList);
				if (currentNode == endNode)
				{
					// Reached final node.
					return CalculatePath(endNode);
				}

				_pathfindOpenList.Remove(currentNode);
				_pathfindClosedList.Add(currentNode);

				SetNeightbourNodes(currentNode, _neighbourSearchType, ref _pathfindTempNeighbours);

				foreach (var neighbourNode in _pathfindTempNeighbours.Values)
				{
					if (_pathfindClosedList.Contains(neighbourNode)) continue;
					if (!_pathfindable.CanBeMovedOver(neighbourNode.GridIndex))
					{
						_pathfindClosedList.Add(neighbourNode);
						continue;
					}

					int tentativeGCost = currentNode.GCost + CalculateDistanceCost(currentNode, neighbourNode);
					if (tentativeGCost < neighbourNode.GCost)
					{
						neighbourNode.PreviousNodeInPathfind = currentNode;
						neighbourNode.GCost = tentativeGCost;
						neighbourNode.HCost = CalculateDistanceCost(neighbourNode, endNode);
						neighbourNode.CalculateFCost();

						if (!_pathfindOpenList.Contains(neighbourNode))
						{
							_pathfindOpenList.Add(neighbourNode);
						}
					}
				}

				loopCount++;
				if (loopCount > 1000)
					break;
			}

			// Out of nodes on open list.
			return null;
		}

		private List<PathNode> CalculatePath(PathNode endNode)
		{
			List<PathNode> path = new List<PathNode> { endNode };

			var currentNode = endNode;
			while (currentNode.PreviousNodeInPathfind != null)
			{
				path.Add(currentNode.PreviousNodeInPathfind);
				currentNode = currentNode.PreviousNodeInPathfind;
			}

			path.Reverse();
			return path;
		}

		private int CalculateDistanceCost(PathNode a, PathNode b)
		{
			int xDistance = Mathf.Abs(a.GridIndex.x - b.GridIndex.x);
			int yDistance = Mathf.Abs(a.GridIndex.y - b.GridIndex.y);
			int remaining = Mathf.Abs(xDistance - yDistance);
			return (MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining);
		}

		private PathNode GetLowestFCostNode(List<PathNode> nodeList)
		{
			var lowestFCostNode = nodeList[0];
			for (int index = 1; index < nodeList.Count; index++)
			{
				if (nodeList[index].FCost < lowestFCostNode.FCost)
					lowestFCostNode = nodeList[index];
			}

			return lowestFCostNode;
		}

		private void SetNeightbourNodes(PathNode node, PathfindNeighbourSearchType searchType, ref Dictionary<Vector3Int, PathNode> neighbours)
		{
			neighbours.Clear();

			Vector3Int cellGridIndex = node.GridIndex;

			Vector3Int neighbourIndex = cellGridIndex + Vector3Int.up;
			if (_pathfindable.IsCellExist(neighbourIndex)) neighbours.Add(neighbourIndex, GetPathNode(neighbourIndex));
			neighbourIndex = cellGridIndex + Vector3Int.right;
			if (_pathfindable.IsCellExist(neighbourIndex)) neighbours.Add(neighbourIndex, GetPathNode(neighbourIndex));
			neighbourIndex = cellGridIndex + Vector3Int.down;
			if (_pathfindable.IsCellExist(neighbourIndex)) neighbours.Add(neighbourIndex, GetPathNode(neighbourIndex));
			neighbourIndex = cellGridIndex + Vector3Int.left;
			if (_pathfindable.IsCellExist(neighbourIndex)) neighbours.Add(neighbourIndex, GetPathNode(neighbourIndex));

			if (searchType == PathfindNeighbourSearchType.AllDirection)
			{
				neighbourIndex = cellGridIndex + new Vector3Int(1, 1, 0);
				if (_pathfindable.IsCellExist(neighbourIndex)) neighbours.Add(neighbourIndex, GetPathNode(neighbourIndex));
				neighbourIndex = cellGridIndex + new Vector3Int(-1, 1, 0);
				if (_pathfindable.IsCellExist(neighbourIndex)) neighbours.Add(neighbourIndex, GetPathNode(neighbourIndex));
				neighbourIndex = cellGridIndex + new Vector3Int(-1, -1, 0);
				if (_pathfindable.IsCellExist(neighbourIndex)) neighbours.Add(neighbourIndex, GetPathNode(neighbourIndex));
				neighbourIndex = cellGridIndex + new Vector3Int(1, -1, 0);
				if (_pathfindable.IsCellExist(neighbourIndex)) neighbours.Add(neighbourIndex, GetPathNode(neighbourIndex));
			}
		}

		private PathNode GetPathNode(Vector3Int celIndex, bool checkCellExists = false)
		{
			if (checkCellExists && !_pathfindable.IsCellExist(celIndex))
				return null;

			if (_tempAllNodes.TryGetValue(celIndex, out PathNode node))
				return node;

            var newNode = new PathNode { GridIndex = celIndex };
			newNode.CalculateFCost();
			_tempAllNodes.Add(celIndex, newNode);

			return newNode;
		}

		private void ClearCache()
		{
			_tempAllNodes.Clear();
			_pathfindClosedList.Clear();
			_pathfindOpenList.Clear();
		}
	}
}