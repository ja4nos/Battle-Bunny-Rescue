using System.Collections.Generic;

namespace BBR.Events.Camera
{
	public class CameraShakeEvent
	{
		public readonly IReadOnlyCollection<int> AffectedPlayers;

		public CameraShakeEvent(IReadOnlyCollection<int> affectedPlayers)
		{
			AffectedPlayers = affectedPlayers;
		}
	}
}
