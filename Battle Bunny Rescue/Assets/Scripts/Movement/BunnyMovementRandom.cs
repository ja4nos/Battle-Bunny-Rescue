using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BBR.Movement
{
	[RequireComponent(typeof(Collider))]
	public class BunnyMovementRandom : BunnyMovementController
	{
		public event Action<Collider> OnBunnyPlayerCollision;

		[SerializeField] private float _directionChangeFrequencySeconds = 2.0f;

		private Vector2 _lastDirection;
		private float? _lastDirectionTime;

		protected override Vector2 GetMovementInput()
		{
			if(!_lastDirectionTime.HasValue
				|| Time.time - _lastDirectionTime >= _directionChangeFrequencySeconds)
			{
				_lastDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0.1f, 1f));
				_lastDirectionTime = Time.time;
			}

			return _lastDirection;
		}

		private void OnTriggerEnter(Collider other)
		{
			if(other.CompareTag("Player"))
			{
				OnBunnyPlayerCollision?.Invoke(other);
			}
		}
	}
}
