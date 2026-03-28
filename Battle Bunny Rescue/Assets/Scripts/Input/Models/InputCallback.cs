using System;
using UnityEngine.InputSystem;

namespace Project.Input.Models
{
	public struct InputCallback : IEquatable<InputCallback>
	{
		public int PlayerId;
		public Action<InputAction.CallbackContext> StartedCallback;
		public Action<InputAction.CallbackContext> PerformedCallback;
		public Action<InputAction.CallbackContext> CanceledCallback;

		public override bool Equals(object obj)
		{
			return obj is InputCallback other && Equals(other);
		}

		public bool Equals(InputCallback other)
		{
			return PlayerId == other.PlayerId
					&& Equals(StartedCallback, other.StartedCallback)
					&& Equals(PerformedCallback, other.PerformedCallback)
					&& Equals(CanceledCallback, other.CanceledCallback);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(PlayerId, StartedCallback, PerformedCallback, CanceledCallback);
		}
	}
}