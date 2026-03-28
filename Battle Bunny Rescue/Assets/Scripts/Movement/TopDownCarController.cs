using Project.Input;
using Project.Input.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

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

		[Header("Jump settings")] [SerializeField]
		private AnimationCurve _jumpCurve;

		[SerializeField] [Min(0.1f)] private float _jumpDuration = 2.0f;
		[SerializeField] private float _fallMultiplier = 25f;

		[Inject] private InputController _inputController;

		private float _accelerationInput;
		private float _steeringInput;
		private float _rotationAngle;
		private Rigidbody _rigidbody;
		private bool _isJumping;

		private InputAction _moveInput;
		private InputCallback _jumpInput;

		private void Start()
		{
			_rigidbody = GetComponent<Rigidbody>();
			_rotationAngle = transform.rotation.eulerAngles.y;

			_inputController.TryGetAction("Move", "Player", out _moveInput);
			_jumpInput = new InputCallback { PlayerId = null, PerformedCallback = Jump };
			_inputController.SubscribeAction("Jump", "Player", _jumpInput);
		}

		private void Update()
		{
			SetInputVector();
		}

		private void FixedUpdate()
		{
			ApplyEngineForce();
			KillOrthogonalVelocity();
			ApplySteering();
		}

		private void ApplyEngineForce()
		{
			if(!ShouldApplyForce())
			{
				return;
			}

			if(!_isJumping && _accelerationInput != 0)
			{
				StartCoroutine(JumpCoroutine(1f));
			}

			_rigidbody.linearDamping = Mathf.Lerp(_rigidbody.linearDamping, _dragMultiplier, Time.deltaTime * _dragMultiplier);
		}

		private bool ShouldApplyForce()
		{
			float velocityVsForward = Vector3.Dot(transform.forward, _rigidbody.linearVelocity);

			if(velocityVsForward > _maxSpeed && _accelerationInput > 0)
			{
				return false;
			}

			if(velocityVsForward < -_maxSpeed * 0.5f && _accelerationInput < 0)
			{
				return false;
			}

			if(_rigidbody.linearVelocity.sqrMagnitude > _maxSpeed * _maxSpeed && _accelerationInput > 0)
			{
				return false;
			}

			return true;
		}

		private void ApplySteering()
		{
			if(!_isJumping && _steeringInput != 0)
			{
				StartCoroutine(JumpCoroutine(1f));
			}

			_rotationAngle += _steeringInput * _turnMultiplier;
			_rigidbody.MoveRotation(Quaternion.AngleAxis(_rotationAngle, Vector3.up));
		}

		private void KillOrthogonalVelocity()
		{
			Vector3 forwardVelocity = transform.forward * Vector3.Dot(_rigidbody.linearVelocity, transform.forward);
			Vector3 rightVelocity = transform.right * Vector3.Dot(_rigidbody.linearVelocity, transform.right);

			_rigidbody.linearVelocity = forwardVelocity + rightVelocity * _driftMultiplier;
		}

		private IEnumerator JumpCoroutine(float jumpHeight)
		{
			_isJumping = true;

			float elapsed = 0f;
			while(elapsed < _jumpDuration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / _jumpDuration;
				float forceFraction = _jumpCurve.Evaluate(t);
				_rigidbody.AddForce(Vector3.up * (jumpHeight * 0.5f * forceFraction), ForceMode.Impulse);

				Vector3 engineForceVector = transform.forward * (_accelerationInput * _accelerationMultiplier);
				_rigidbody.AddForce(engineForceVector, ForceMode.Impulse);

				yield return null;
			}

			yield return new WaitUntil(() => _rigidbody.linearVelocity.y < 0);

			elapsed = 0f;

			while(_isJumping)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / _jumpDuration;
				_rigidbody.AddForce(Vector3.down * _fallMultiplier, ForceMode.Force);
				yield return null;
			}

			_isJumping = false;
		}

		private void OnCollisionEnter(Collision collision)
		{
			if(collision.gameObject.CompareTag("Ground"))
			{
				_isJumping = false;
			}
		}

		public void SetInputVector()
		{
			if(_moveInput != null)
			{
				Vector2 inputVector = _moveInput.ReadValue<Vector2>();
				_steeringInput = inputVector.x;
				_accelerationInput = inputVector.y;
			}
		}

		public void Jump(InputAction.CallbackContext context)
		{
			if(context.performed && !_isJumping)
			{
				StartCoroutine(JumpCoroutine(1f));
			}
		}

		private void OnDestroy()
		{
			_inputController.UnsubscribeAction("Jump", "Player", _jumpInput);
		}
	}
}
