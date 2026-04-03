using Project.Input;
using Project.Input.Models;
using UnityEngine;
using Zenject;

namespace BBR.Movement
{
	public class BunnyMovementPlayer : BunnyMovementController
	{
		[Inject] private InputController _inputController;

		private InputCallback _jumpInput;
		private int _playerId;

		public void Init(int playerId)
		{
			_playerId = playerId;

			_jumpInput = new InputCallback { PlayerId = _playerId, PerformedCallback = Jump };
			_inputController.SubscribeAction("Jump", "Player", _jumpInput);
		}

		protected override Vector2 GetMovementInput()
		{
			return _inputController.TryReadValue("Move", "Player", _playerId, out Vector2 inputVector)
				? inputVector
				: Vector2.zero;
		}

		private void OnDestroy()
		{
			_inputController.UnsubscribeAction("Jump", "Player", _jumpInput);
		}
	}
}
