using UnityEngine;

namespace Project.Utilities
{
	public static class VectorExtensions
	{
		public static Vector3 ReplaceX(this Vector3 v, float x)
		{
			return new Vector3(x, v.y, v.z);
		}

		public static Vector3 ReplaceY(this Vector3 v, float y)
		{
			return new Vector3(v.x, y, v.z);
		}

		public static Vector3 ReplaceZ(this Vector3 v, float z)
		{
			return new Vector3(v.x, v.y, z);
		}

		public static Vector2 ReplaceX(this Vector2 v, float x)
		{
			return new Vector2(x, v.y);
		}

		public static Vector2 ReplaceY(this Vector2 v, float y)
		{
			return new Vector2(v.x, y);
		}
	}
}
