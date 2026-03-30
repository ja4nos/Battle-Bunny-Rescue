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

		public void Init(int playerId, InputController inputController)
		{
			_playerId = playerId;
			_inputController = inputController;
			PlayerIndex = _playerId;
		}

		private void Update()
		{
			if(!_inputController.TryReadValue("Look", _actionMapName, _playerId, out Vector2 look))
			{
				return;
			}

			Debug.Log($"LOOK {_playerId} {look}");

			CinemachineCamera freeLook = GetComponent<CinemachineCamera>();
			if(freeLook == null)
			{
				return;
			}

			if(freeLook.TryGetComponent(out CinemachineOrbitalFollow orbital))
			{
				orbital.HorizontalAxis.Value += look.x * 0.1f;
				orbital.VerticalAxis.Value += look.y * 0.1f;
			}
		}
	}
}