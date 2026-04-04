using Project.Input;
using Unity.Cinemachine;
using UnityEngine;

namespace BBR.CameraController
{
	public class PlayerCinemachineInputProvider : CinemachineInputAxisController
	{
		[SerializeField] private string _lookXActionName = "LookX";
		[SerializeField] private string _lookYActionName = "LookY";
		[SerializeField] private string _actionMapName = "Player";

		private InputAxis _horizontalAxis;
		private InputAxis _verticalAxis;
		private int _playerId;
		private InputController _inputController;
 		private CinemachineOrbitalFollow _orbitalFollow;

		public void Init(int playerId, InputController inputController)
		{
			_playerId = playerId;
			_inputController = inputController;
			PlayerIndex = _playerId;

			if(!transform.TryGetComponent(out _orbitalFollow))
			{
				Debug.LogError($"Could not find {nameof(CinemachineOrbitalFollow)} on player {name}! Disabling the {nameof(PlayerCinemachineInputProvider)} script!", this);
			}
		}

		private void Update()
		{
			if(_inputController == null || !_inputController.TryReadValue("Look", _actionMapName, _playerId, out Vector2 look))
			{
				return;
			}
			_orbitalFollow.HorizontalAxis.Value += look.x * 0.1f;
			_orbitalFollow.VerticalAxis.Value += look.y * 0.1f;
		}
	}
}
