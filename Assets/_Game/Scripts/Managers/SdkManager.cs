using GameEngine.Library.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Game.Core
{
    public class SdkManager : PersistentSingleton<SdkManager>, EventListener<BoardElementEvent>
																, EventListener<BoardElementPlacementEvent>
																, EventListener<BoardElementSelectEvent>
    {
		public void OnCGEvent(BoardElementEvent currentEvent)
		{
			Color color = Color.white;
			switch (currentEvent.EventType)
			{
				case BoardElementEventType.Damaged: color = new Color(255f / 255f, 105f / 255f, 180f / 255f, 1f); break;	// Pink
				case BoardElementEventType.Destroyed: color = new Color(143f / 255f, 45f / 255f, 194f / 255f, 1f); break;	// Purple
			}

			// Send to SDK API.
			GEDebug.LogColored($"BoardElementEvent: {currentEvent.EventType}, {currentEvent.BoardElement.PlacableData.Name}, {currentEvent.BoardElement.FightingSide}, Damage: {currentEvent.Damage}, Remaining: {currentEvent.RemainingHealth}", color: color);
		}

		public void OnCGEvent(BoardElementPlacementEvent currentEvent)
		{
			Color color = Color.white;
			switch (currentEvent.Status)
			{
				case BoardElementPlacementStatus.Placed: color = Color.green; break;
				case BoardElementPlacementStatus.Failed: color = Color.yellow; break;
			}

			GEDebug.LogColored($"BoardElementPlacementEvent: {currentEvent.Status}, {currentEvent.BoardElement.PlacableData.Name}, {currentEvent.BoardElement.FightingSide}", color: color);
		}

		public void OnCGEvent(BoardElementSelectEvent currentEvent)
		{
			string message = currentEvent.Status ? "Selected" : "Deselected";
			Debug.Log($"BoardElementSelectEvent: {message}, {currentEvent.BoardElement.PlacableData.Name}, {currentEvent.BoardElement.FightingSide}");
		}

		private void OnEnable()
		{
			this.EventStartListening<BoardElementEvent>();
			this.EventStartListening<BoardElementPlacementEvent>();
			this.EventStartListening<BoardElementSelectEvent>();
		}

		private void OnDisable()
		{
			this.EventStopListening<BoardElementEvent>();
			this.EventStopListening<BoardElementPlacementEvent>();
			this.EventStopListening<BoardElementSelectEvent>();
		}
	}
}