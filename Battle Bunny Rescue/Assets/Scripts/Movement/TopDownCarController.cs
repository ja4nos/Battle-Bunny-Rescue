using UnityEngine;

namespace BBR
{
	[RequireComponent(typeof(Rigidbody))]
	public class TopDownCarController : MonoBehaviour
	{
		[Header("Car settings")] [SerializeField]
		private float _accelerationMultiplier = 30.0f;

		[SerializeField] private float _turnMultiplier = 3.5f;
		[SerializeField] private float _driftMultiplier = 0.95f;
		[SerializeField] private float _inPlaceRotationDivider = 8f;
		[SerializeField] private float _dragMultiplier = 3.0f;
		[SerializeField] private float _maxSpeed = 20f;

		private float _accelerationInput;
		private float _steeringInput;
		private float _rotationAngle;
		private float _velocityVsUp;
		private Rigidbody _rigidbody;

		private void Start()
		{
			_rigidbody = GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			ApplyEngineForce();
			KillOrthogonalVelocity();
			ApplySteering();
		}

		private void ApplyEngineForce()
		{
			_velocityVsUp = Vector3.Dot(transform.up, _rigidbody.linearVelocity);

			if(_velocityVsUp > _maxSpeed && _accelerationInput > 0)
			{
				return;
			}

			if(_velocityVsUp < -_maxSpeed * 0.5f && _accelerationInput < 0)
			{
				return;
			}

			if(_rigidbody.linearVelocity.sqrMagnitude > _maxSpeed * _maxSpeed && _accelerationInput > 0)
			{
				return;
			}

			_rigidbody.linearDamping = _accelerationInput == 0
				? Mathf.Lerp(_rigidbody.linearDamping, _dragMultiplier, Time.deltaTime * _dragMultiplier)
				: 0;

			Vector3 engineForceVector = transform.forward * _accelerationInput * _accelerationMultiplier;
			_rigidbody.AddForce(engineForceVector, ForceMode.Force);
		}

		private void ApplySteering()
		{
			_rotationAngle += _steeringInput * _turnMultiplier * GetInPlaceDivider();
			_rigidbody.MoveRotation(Quaternion.AngleAxis(_rotationAngle, Vector3.up));
		}

		private float GetInPlaceDivider()
		{
			if(_inPlaceRotationDivider > 0)
			{
				//TODO: Not sure if we need it as bunnies can technically hop in place horizontally
				float minSpeedTurn = _rigidbody.linearVelocity.magnitude / _inPlaceRotationDivider;
				return Mathf.Clamp01(minSpeedTurn);
			}
			return 1f;
		}

		private void KillOrthogonalVelocity()
		{
			Vector3 forwardVelocity = transform.forward * Vector3.Dot(_rigidbody.linearVelocity, transform.forward);
			Vector3 rightVelocity = transform.right * Vector3.Dot(_rigidbody.linearVelocity, transform.right);

			_rigidbody.linearVelocity = forwardVelocity + rightVelocity * _driftMultiplier;
		}

		public void SetInputVector(Vector2 inputVector)
		{
			_steeringInput = inputVector.x;
			_accelerationInput = inputVector.y;
		}
	}
}
