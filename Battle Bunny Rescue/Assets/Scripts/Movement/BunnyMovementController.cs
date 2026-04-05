using BBR.AudioPlayer;
using BBR.Movement.Enums;
using BBR.Movement.Helpers;
using Pool.Pool;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace BBR.Movement
{
	[RequireComponent(typeof(Rigidbody))]
	public abstract class BunnyMovementController : MonoBehaviour
	{
		[Header("Car settings")] [SerializeField]
		private float _accelerationMultiplier = 30.0f;

		[SerializeField] private float _turnMultiplier = 3.5f;
		[SerializeField] private float _driftMultiplier = 0.95f;
		[SerializeField] private float _dragMultiplier = 3.0f;
		[SerializeField] private float _maxSpeed = 20f;

		[Header("Jump settings")] [SerializeField] [FormerlySerializedAs("_jumpCurve")]
		protected AnimationCurve JumpCurve;

		[SerializeField] [Min(0.1f)] [FormerlySerializedAs("_jumpDuration")]
		protected float JumpDuration = 2.0f;

		[SerializeField] [FormerlySerializedAs("_hopHeight")]
		protected float HopHeight = 0.5f;

		[SerializeField] private float _fallSpeed = 10f;

		[SerializeField] [FormerlySerializedAs("_visualTransform")]
		public Transform VisualTransform;

		[SerializeField] [FormerlySerializedAs("_animator")]
		protected Animator Animator;

		[SerializeField] private Collider _collider;

		[SerializeField] protected ParticlePool DustParticlePool;

		[SerializeField] protected AudioHolder HopSound;

		protected IEnumerator HopCoroutine;
		protected MovementStatus CurrentState;
		protected Rigidbody Rigidbody;

		private float _accelerationInput;
		private float _steeringInput;
		private float _rotationAngle;
		private int _groundMask;

		protected virtual void Start()
		{
			Rigidbody = GetComponent<Rigidbody>();
			_rotationAngle = transform.rotation.eulerAngles.y;
			_groundMask = 1 << LayerMask.NameToLayer("Ground");
		}

		protected virtual void Update()
		{
			SetInputVector();
			FallDown();
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

			Rigidbody.linearDamping = Mathf.Lerp(Rigidbody.linearDamping, _dragMultiplier, Time.deltaTime * _dragMultiplier);
			Vector3 engineForceVector = transform.forward * (_accelerationInput * _accelerationMultiplier);
			Rigidbody.AddForce(engineForceVector, ForceMode.Force);
		}

		private bool ShouldApplyForce()
		{
			float velocityVsForward = Vector3.Dot(transform.forward, Rigidbody.linearVelocity);

			if(velocityVsForward > _maxSpeed && _accelerationInput > 0)
			{
				return false;
			}

			if(velocityVsForward < -_maxSpeed * 0.5f && _accelerationInput < 0)
			{
				return false;
			}

			if(Rigidbody.linearVelocity.sqrMagnitude > _maxSpeed * _maxSpeed && _accelerationInput > 0)
			{
				return false;
			}

			return true;
		}

		private void ApplySteering()
		{
			_rotationAngle += _steeringInput * _turnMultiplier;
			Rigidbody.MoveRotation(Quaternion.AngleAxis(_rotationAngle, Vector3.up));
		}

		private void KillOrthogonalVelocity()
		{
			Vector3 forwardVelocity = transform.forward * Vector3.Dot(Rigidbody.linearVelocity, transform.forward);
			Vector3 rightVelocity = transform.right * Vector3.Dot(Rigidbody.linearVelocity, transform.right);

			Rigidbody.linearVelocity = forwardVelocity + rightVelocity * _driftMultiplier;
		}

		protected virtual IEnumerator Hop()
		{
			MovementHelper.AddState(ref CurrentState, MovementStatus.Hopping);

			float elapsed = 0f;
			while(elapsed < JumpDuration)
			{
				elapsed += Time.deltaTime;
				float t = JumpCurve.Evaluate(elapsed / JumpDuration);
				VisualTransform.localPosition = new Vector3(0, t * HopHeight, 0);
				yield return null;
			}

			VisualTransform.localPosition = Vector3.zero;
			ParticleSystem particles = DustParticlePool.Get();
			particles.transform.position = VisualTransform.position;
			particles.Play();
			if(HopSound)
			{
				HopSound.Play();
			}
			MovementHelper.RemoveState(ref CurrentState, MovementStatus.Hopping);
		}

		private void FallDown()
		{
			float? maxHitPoint = null;
			Vector3 colliderSize = transform.lossyScale;

			Vector3 rayOrigin = transform.position + Vector3.up * 100f;

			if(Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1000f, _groundMask))
			{
				maxHitPoint = hit.point.y;
			}

			rayOrigin = _collider.bounds.center + colliderSize + Vector3.up * 100f;

			if(Physics.Raycast(rayOrigin, Vector3.down, out hit, 1000f, _groundMask))
			{
				maxHitPoint = maxHitPoint.HasValue ? Mathf.Max(hit.point.y, maxHitPoint.Value) : hit.point.y;
			}

			rayOrigin = _collider.bounds.center - colliderSize + Vector3.up * 100f;

			if(Physics.Raycast(rayOrigin, Vector3.down, out hit, 1000f, _groundMask))
			{
				maxHitPoint = maxHitPoint.HasValue ? Mathf.Max(hit.point.y, maxHitPoint.Value) : hit.point.y;
			}

			if(maxHitPoint.HasValue)
			{
				if(maxHitPoint.Value < transform.position.y)
				{
					float step = _fallSpeed * Time.deltaTime;
					float decrease = Math.Min(transform.position.y - maxHitPoint.Value, step);
					transform.position -= new Vector3(0, decrease, 0);
				}
				else
				{
					transform.position = new Vector3(transform.position.x, maxHitPoint.Value, transform.position.z);

					if(CurrentState.HasFlag(MovementStatus.Bumped) && !CurrentState.HasFlag(MovementStatus.Recoil))
					{
						MovementHelper.RemoveState(ref CurrentState, MovementStatus.Bumped);
						OnPlayerStoppedBumping();
					}
				}
			}
		}

		public virtual void SetInputVector()
		{
			Vector2 inputVector = GetMovementInput();
			_steeringInput = inputVector.x;
			_accelerationInput = inputVector.y;

			if(CurrentState != MovementStatus.Bumped && !MovementHelper.IsAirborne(CurrentState)
													&& (_accelerationInput != 0 || _steeringInput != 0))
			{
				if(HopCoroutine != null)
				{
					StopCoroutine(HopCoroutine);
				}

				HopCoroutine = Hop();
				StartCoroutine(HopCoroutine);
			}
		}

		protected virtual void OnPlayerStoppedBumping() { }

		protected abstract Vector2 GetMovementInput();

		protected virtual void OnDestroy()
		{
			DustParticlePool.Dispose();
		}
	}
}
