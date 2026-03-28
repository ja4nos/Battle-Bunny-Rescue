using Project.Input.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Project.Input
{
	public class InputTester : MonoBehaviour
	{
		private InputController _inputController;

		[Inject]
		private void Construct(InputController inputController)
		{
			_inputController = inputController;
		}
		
		private void Awake()
		{
			int? keyboardId = Keyboard.current?.deviceId;
			int? gamepadId = Gamepad.current?.deviceId;
			
			if(keyboardId.HasValue)
			{
				_inputController.RegisterDevice(0, keyboardId.Value);
			}

			if(gamepadId.HasValue)
			{
				_inputController.RegisterDevice(1, gamepadId.Value);
			}

			_inputController.SubscribeAction("Move", "Player", new InputCallback
			{
				PlayerId = 0,
				StartedCallback = Callback,
				PerformedCallback = Callback,
				CanceledCallback = Callback
			});

			_inputController.SubscribeAction("Move", "Player", new InputCallback
			{
				PlayerId = 1,
				StartedCallback = Callback,
				PerformedCallback = Callback,
				CanceledCallback = Callback
			});
			
			Debug.Log($"Tester subscribed with keyboard {keyboardId?.ToString() ?? "Null"} and gamepad {gamepadId?.ToString() ?? "Null"}");
		}

		private static void Callback(InputAction.CallbackContext obj)
		{
			Debug.Log($"[{obj.control.device}] {obj.phase.ToString()}");
		}
	}
}