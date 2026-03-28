using UnityEngine;
using UnityEngine.InputSystem;

namespace BBR.Camera.Prototype
{
	[RequireComponent(typeof(Rigidbody))]
	public class PlayerController : MonoBehaviour
	{
		[SerializeField] private float _acceleration = 1.0f;
		[SerializeField] private float _turnSpeed = 1.0f;
		[SerializeField] private float _maxSpeed = 1.0f;
		[SerializeField] private float _friction = 1.0f;

		private Rigidbody _rigidbody;
		private float _currentSpeed;

		private void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
		}

		private void Update()
		{
			float desiredSpeed = _currentSpeed * (100 / _friction) * Time.deltaTime;
			if(Keyboard.current.wKey.isPressed)
			{
				desiredSpeed = _currentSpeed + _acceleration * Time.deltaTime;
			}
			else if(Keyboard.current.aKey.isPressed)
			{
				desiredSpeed = _currentSpeed - _acceleration * Time.deltaTime;
			}

			if(Keyboard.current.dKey.isPressed)
			{
				_rigidbody.rotation *= Quaternion.Euler(0, _turnSpeed * Time.deltaTime, 0);
			}
			else if(Keyboard.current.aKey.isPressed)
			{
				_rigidbody.rotation *= Quaternion.Euler(0, -_turnSpeed * Time.deltaTime, 0);
			}

			_currentSpeed = Mathf.Clamp(desiredSpeed, -_maxSpeed, _maxSpeed);
			_rigidbody.linearVelocity = _currentSpeed * transform.forward;
		}
	}
}
