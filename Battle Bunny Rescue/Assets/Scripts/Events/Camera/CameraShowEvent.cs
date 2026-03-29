using UnityEngine;

namespace BBR.Events.Camera
{
	public class CameraShowEvent
	{
		public readonly Vector3 Position;
		public readonly Transform Transform;
		public readonly float Time;

		public CameraShowEvent(Transform transform, float time)
		{
			Transform = transform;
			Time = time;
		}

		public CameraShowEvent(Vector3 position, float time)
		{
			Position = position;
			Time = time;
		}
	}
}
