using System;
using UnityEngine;

namespace BBR
{
	[RequireComponent(typeof(Rigidbody))]
    public class TopDownCarController : MonoBehaviour
    {
        [Header("Car settings")]
        [SerializeField] private float _accelerationMultiplier = 30.0f;
         [SerializeField] private float _turnMultiplier = 3.5f;

		private float _accelerationInput;
		private float _steeringInput;
		private float _rotationAngle;
		private Rigidbody _rigidbody;
		
		private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

		private void FixedUpdate()
		{
			ApplyEngineForce();
			ApplySteering();
		}

		private void ApplyEngineForce()
		{
			Vector3 engineForceVector = transform.forward * _accelerationInput * _accelerationMultiplier;
			_rigidbody.AddForce(engineForceVector, ForceMode.Force);
		}

		private void ApplySteering()
		{
			_rotationAngle += _steeringInput * _turnMultiplier;
			_rigidbody.MoveRotation(Quaternion.AngleAxis(_rotationAngle, Vector3.up));
		}
		
		public void SetInputVector(Vector2 inputVector)
		{
			_steeringInput = inputVector.x;
			_accelerationInput = inputVector.y;
		}
	}
}
