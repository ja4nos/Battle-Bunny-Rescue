using BBR.Events;
using BBR.Events.Camera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BBR.CameraController.Prototype
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

			if(Keyboard.current.eKey.isPressed)
			{
				PlayerController[] players = FindObjectsByType<PlayerController>();
				Transform t = players[Random.Range(0, players.Length)].transform;

				CameraShowEvent cameraShowEvent = new(t, Random.Range(2, 5));
				EventBus.Fire(cameraShowEvent);
			}

			if(Keyboard.current.rKey.isPressed)
			{
				PlayerController[] players = FindObjectsByType<PlayerController>();
				Vector3 pos = players[Random.Range(0, players.Length)].transform.position;

				CameraShowEvent cameraShowEvent = new(pos, Random.Range(2, 5));
				EventBus.Fire(cameraShowEvent);
			}

			_currentSpeed = Mathf.Clamp(desiredSpeed, -_maxSpeed, _maxSpeed);
			_rigidbody.linearVelocity = _currentSpeed * transform.forward;
		}
	}
}
