using UnityEngine;

namespace GameEngine.Game.Pathfinding
{
    public class PathNode
    {
		public Vector3Int GridIndex { get; set; }
		public int GCost { get; set; } = int.MaxValue;
		public int HCost { get; set; }
		public int FCost { get; set; }

		public PathNode PreviousNodeInPathfind { get; set; } = null;

		public void CalculateFCost()
		{
			FCost = GCost + HCost;
		}

		public static bool operator ==(PathNode n1, PathNode n2)
		{
			if ((object)n1 == null)
				return (object)n2 == null;

			return n1.Equals(n2);
		}

		public static bool operator !=(PathNode n1, PathNode n2)
		{
			return !(n1 == n2);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			var n2 = (PathNode)obj;
			return (GridIndex == n2.GridIndex);
		}

		public override int GetHashCode()
		{
			return GridIndex.GetHashCode();
		}
	}
}