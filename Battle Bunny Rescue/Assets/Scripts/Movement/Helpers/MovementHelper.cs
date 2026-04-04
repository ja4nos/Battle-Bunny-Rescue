using BBR.Movement.Enums;
using System.Collections.Generic;

namespace BBR.Movement.Helpers
{
	public static class MovementHelper
	{
		private static readonly Dictionary<MovementStatus, MovementStatus> _exclusiveGroups = new()
		{
			{ MovementStatus.Jumping, MovementStatus.Hopping },
			{ MovementStatus.Hopping, MovementStatus.Jumping }
		};

		public static bool IsAirborne(MovementStatus currentState) => (currentState & MovementStatus.AnyAirborne) != 0;

		public static void AddState(ref MovementStatus currentState, MovementStatus toAdd)
		{
			if(_exclusiveGroups.TryGetValue(toAdd, out MovementStatus toRemove))
			{
				RemoveState(ref currentState, toRemove);
			}

			currentState |= toAdd;
		}

		public static void RemoveState(ref MovementStatus currentState, MovementStatus toRemove)
		{
			currentState &= ~toRemove;
		}
	}
}
