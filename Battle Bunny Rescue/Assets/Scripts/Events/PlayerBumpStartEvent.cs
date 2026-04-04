using System.Collections.Generic;
using UnityEngine;

namespace BBR.Events
{
	public class PlayerBumpStartEvent
	{
		public readonly Vector3 Position;
		public readonly IReadOnlyCollection<int> PlayerIndices;

		public PlayerBumpStartEvent(Vector3 position, IReadOnlyCollection<int> playerIndices)
		{
			Position = position;
			PlayerIndices = playerIndices;
		}
	}
}
