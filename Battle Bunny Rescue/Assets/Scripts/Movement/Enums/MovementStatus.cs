using System;

namespace BBR.Movement.Enums
{
	[Flags]
	public enum MovementStatus
	{
		None = 0,
		Hopping = 1 << 0,
		Jumping = 1 << 1,
		Sprinting = 1 << 2,
		Bumped = 1 << 3,
		Recoil = 1 << 4,
		AnyAirborne = Jumping | Hopping
	}
}
