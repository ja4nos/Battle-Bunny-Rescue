using System.Collections.Generic;

namespace BBR.Events.Camera
{
	public class CameraShakeEvent
	{
		public readonly float Force;
		public readonly IReadOnlyCollection<int> AffectedPlayers;

		public CameraShakeEvent(float force, IReadOnlyCollection<int> affectedPlayers)
		{
			Force = force;
			AffectedPlayers = affectedPlayers;
		}
	}
}
