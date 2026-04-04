using BBR.Events;
using BBR.Movement.Enums;
using BBR.Movement.Helpers;
using Project.Input;
using Project.Input.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace BBR.Movement
{
	public class BunnyMovementPlayer : BunnyMovementController
	{
		[SerializeField] private float _jumpMultiplier = 4f;
		[SerializeField] private float _staminaTimeSeconds = 4f;
		[SerializeField] private float _staminaRecoveryRatePerSecond = 1f;
		[SerializeField] private float _sprintMultiplier = 4f;
		[SerializeField] private Vector2 _bumpForce = new(2f, 2f);

		[Inject] private InputController _inputController;

		private InputCallback _jumpInput;
		private InputCallback _sprintInput;
		private int _playerId;
		private IEnumerator _jumpCoroutine;
		private IEnumerator _bumpCoroutine;
		private float _remainingStamina;

		public void Init(int playerId)
		{
			_playerId = playerId;

			_jumpInput = new InputCallback { PlayerId = _playerId, PerformedCallback = Jump };
			_inputController.SubscribeAction("Jump", "Player", _jumpInput);
		}

		protected override void Update()
		{
			_remainingStamina = Mathf.Min(_remainingStamina + Time.deltaTime * _staminaRecoveryRatePerSecond, _staminaTimeSeconds);
			base.Update();
		}

		protected override Vector2 GetMovementInput()
		{
			Vector2 sprintMultiplier = Vector2.one;

			if(_inputController.TryReadValue("Sprint", "Player", _playerId, out float sprintValue)
				&& sprintValue > 0f)
			{
				_remainingStamina = Mathf.Max(_remainingStamina - Time.deltaTime, 0);
				if(_remainingStamina > 0)
				{
					sprintMultiplier.y = _sprintMultiplier;
				}
			}

			return _inputController.TryReadValue("Move", "Player", _playerId, out Vector2 inputVector)
				? Vector2.Scale(inputVector, sprintMultiplier)
				: Vector2.zero;
		}

		private void Jump(InputAction.CallbackContext context)
		{
			if(context.performed && !CurrentState.HasFlag(MovementStatus.Jumping)
				&& !CurrentState.HasFlag(MovementStatus.Bumped))
			{
				if(HopCoroutine != null)
				{
					StopCoroutine(HopCoroutine);
				}

				if(_jumpCoroutine != null)
				{
					StopCoroutine(_jumpCoroutine);
				}

				_jumpCoroutine = JumpCoroutine();
				StartCoroutine(_jumpCoroutine);
			}
		}

		private IEnumerator JumpCoroutine()
		{
			MovementHelper.AddState(ref CurrentState, MovementStatus.Jumping);

			float elapsed = 0f;
			float jumpHeight = HopHeight * _jumpMultiplier;
			float jumpDuration = JumpDuration * (_jumpMultiplier / 2f);
			float initialHeight = VisualTransform.localPosition.y;

			while(elapsed < jumpDuration)
			{
				elapsed += Time.deltaTime;
				float t = JumpCurve.Evaluate(elapsed / jumpDuration);
				VisualTransform.localPosition = new Vector3(0, initialHeight + t * jumpHeight, 0);
				yield return null;
			}

			VisualTransform.localPosition = Vector3.zero;
			ParticleSystem particles = DustParticlePool.Get();
			particles.transform.position = VisualTransform.position;
			particles.Play();
			MovementHelper.RemoveState(ref CurrentState, MovementStatus.Jumping);
		}

		private void OnTriggerEnter(Collider other)
		{
			if(other.transform.CompareTag("Player"))
			{
				other.GetComponentInParent<BunnyMovementPlayer>().Bump(transform.forward);
			}
		}

		public void Bump(Vector3 direction)
		{
			if(HopCoroutine != null)
			{
				StopCoroutine(HopCoroutine);
			}

			if(_jumpCoroutine != null)
			{
				StopCoroutine(_jumpCoroutine);
			}

			_bumpCoroutine = BumpCoroutine(direction);
			StartCoroutine(_bumpCoroutine);
		}

		private IEnumerator BumpCoroutine(Vector3 direction)
		{
			CurrentState = MovementStatus.Bumped;
			MovementHelper.AddState(ref CurrentState, MovementStatus.Recoil);

			float elapsed = 0f;
			float jumpHeight = HopHeight * _jumpMultiplier * _bumpForce.y;
			float jumpDuration = JumpDuration * (_jumpMultiplier / 2f);
			float initialHeight = VisualTransform.localPosition.y;

			while(elapsed < jumpDuration)
			{
				elapsed += Time.deltaTime;
				float t = JumpCurve.Evaluate(elapsed / jumpDuration);
				VisualTransform.localPosition = new Vector3(0, initialHeight + t * jumpHeight, 0);
				Rigidbody.AddForce(direction * _bumpForce.x, ForceMode.Impulse);
				yield return null;
			}

			VisualTransform.localPosition = Vector3.zero;
			MovementHelper.RemoveState(ref CurrentState, MovementStatus.Recoil);
		}

		protected override void OnPlayerStoppedBumping()
		{
			base.OnPlayerStoppedBumping();
			EventBus.Fire(new PlayerBumpedEvent(_playerId));
		}

		protected override void OnDestroy()
		{
			_inputController.UnsubscribeAction("Jump", "Player", _jumpInput);
			base.OnDestroy();
		}
	}
}
