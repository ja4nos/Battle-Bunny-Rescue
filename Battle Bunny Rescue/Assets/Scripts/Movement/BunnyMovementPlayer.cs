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

		[Inject] private InputController _inputController;

		private InputCallback _jumpInput;
		private InputCallback _sprintInput;
		private int _playerId;
		private IEnumerator _jumpCoroutine;
		private float _remainingStamina;
		private StaminaChangedEvent _staminaChangedEvent;

		public void Init(int playerId)
		{
			_playerId = playerId;
			_staminaChangedEvent = new StaminaChangedEvent(playerId);

			_jumpInput = new InputCallback { PlayerId = _playerId, PerformedCallback = Jump };
			_inputController.SubscribeAction("Jump", "Player", _jumpInput);

			_remainingStamina = _staminaTimeSeconds;
		}

		protected override void Update()
		{
			_remainingStamina = Mathf.Min(_remainingStamina + Time.deltaTime * _staminaRecoveryRatePerSecond, _staminaTimeSeconds);

			_staminaChangedEvent.StaminaPercentage = _remainingStamina / _staminaTimeSeconds * 100f;
			EventBus.Fire(_staminaChangedEvent);

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

				_staminaChangedEvent.StaminaPercentage = _remainingStamina / _staminaTimeSeconds * 100f;
				EventBus.Fire(_staminaChangedEvent);
			}

			return _inputController.TryReadValue("Move", "Player", _playerId, out Vector2 inputVector)
				? Vector2.Scale(inputVector, sprintMultiplier)
				: Vector2.zero;
		}

		private void Jump(InputAction.CallbackContext context)
		{
			if(context.performed && !CurrentState.HasFlag(MovementStatus.Jumping))
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

		protected override void OnDestroy()
		{
			_inputController.UnsubscribeAction("Jump", "Player", _jumpInput);
			base.OnDestroy();
		}
	}
}