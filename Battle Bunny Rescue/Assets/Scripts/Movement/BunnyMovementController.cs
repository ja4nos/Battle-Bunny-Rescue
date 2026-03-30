using Project.Input;
using Project.Input.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace BBR.Movement
{
	[RequireComponent(typeof(Rigidbody))]
	public class BunnyMovementController : MonoBehaviour
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

		[SerializeField] private AnimationCurve _fallCurve;
		[SerializeField] [Min(0.1f)] private float _jumpDuration = 2.0f;
		[SerializeField] private float _jumpDownDuration = 0.2f;
		[SerializeField] private float _jumpHeight = 0.5f;
		[SerializeField] private Transform _visualTransform;
		[SerializeField] private Animator _animator;

		[Inject] private InputController _inputController;

		private int _playerId;
		private float _accelerationInput;
		private float _steeringInput;
		private float _rotationAngle;
		private Rigidbody _rigidbody;
		private bool _isJumping;
		private int _groundMask;

		private InputCallback _jumpInput;

		private void Start()
		{
			_rigidbody = GetComponent<Rigidbody>();
			_rotationAngle = transform.rotation.eulerAngles.y;
			_groundMask = 1 << LayerMask.NameToLayer("Ground");
		}

		public void Init(int playerId)
		{
			_playerId = playerId;

			_jumpInput = new InputCallback {PlayerId = _playerId, PerformedCallback = Jump};
			_inputController.SubscribeAction("Jump", "Player", _jumpInput);
		}

		private void Update()
		{
			SetInputVector();
		}

		private void FixedUpdate()
		{
			FallDown();
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

			_rigidbody.linearDamping = Mathf.Lerp(_rigidbody.linearDamping, _dragMultiplier, Time.deltaTime * _dragMultiplier);
			Vector3 engineForceVector = transform.forward * (_accelerationInput * _accelerationMultiplier);
			_rigidbody.AddForce(engineForceVector, ForceMode.Force);
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
			_rotationAngle += _steeringInput * _turnMultiplier;
			_rigidbody.MoveRotation(Quaternion.AngleAxis(_rotationAngle, Vector3.up));
		}

		private void KillOrthogonalVelocity()
		{
			Vector3 forwardVelocity = transform.forward * Vector3.Dot(_rigidbody.linearVelocity, transform.forward);
			Vector3 rightVelocity = transform.right * Vector3.Dot(_rigidbody.linearVelocity, transform.right);

			_rigidbody.linearVelocity = forwardVelocity + rightVelocity * _driftMultiplier;
		}

		private IEnumerator JumpCoroutine()
		{
			_isJumping = true;

			float elapsed = 0f;
			while(elapsed < _jumpDuration)
			{
				elapsed += Time.deltaTime;
				float t = _jumpCurve.Evaluate(elapsed / _jumpDuration);
				_visualTransform.localPosition = new Vector3(0, t * _jumpHeight, 0);
				yield return null;
			}

			_visualTransform.localPosition = Vector3.zero;
			_isJumping = false;
		}

		private void FallDown()
		{
			if(!_isJumping)
			{
				Vector3 rayOrigin = transform.position + Vector3.up * 0.1f; // slight offset up

				if(Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 10f, _groundMask))
				{
					if(hit.point.y < transform.position.y)
					{
						transform.position -= new Vector3(0, transform.position.y - hit.point.y, 0);
					}
				}
			}
		}

		public void SetInputVector()
		{
			if(_inputController.TryReadValue("Move", "Player", _playerId, out Vector2 inputVector))
			{
				_steeringInput = inputVector.x;
				_accelerationInput = inputVector.y;
			}

			if(!_isJumping && (_accelerationInput != 0 || _steeringInput != 0))
			{
				StartCoroutine(JumpCoroutine());
			}
		}

		public void Jump(InputAction.CallbackContext context)
		{
			if(context.performed && !_isJumping)
			{
				StartCoroutine(JumpCoroutine());
			}
		}

		private void OnDestroy()
		{
			_inputController.UnsubscribeAction("Jump", "Player", _jumpInput);
		}
	}
}