using Project.Input;
using Project.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Zenject;

namespace Project.Menu
{
	public class PlayerSelectionMenuController : MonoBehaviour
	{
		[SerializeField] private UIDocument _menuUIDocument;
		[SerializeField] private SceneGroup _gameSceneGroup;

		[Inject] private InputController _inputController;
		[Inject] private DiContainer _diContainer;

		private VisualElement _startBindingVisuals;
		private readonly Dictionary<int, PlayerConnectionController> _playerConnections = new();

		private void Awake()
		{
			if(_menuUIDocument == null)
			{
				Debug.LogError($"No main menu UI document has been assigned to {nameof(PlayerSelectionMenuController)}!", this);
			}

			InputSystem.onAnyButtonPress.Call(OnDeviceButtonPress);

			_startBindingVisuals = _menuUIDocument.rootVisualElement.Q(className: "binding", name: "ready-start");
			_playerConnections.Add(0, new PlayerConnectionController(_menuUIDocument.rootVisualElement.Q(name: "player-1")));
			_playerConnections.Add(1, new PlayerConnectionController(_menuUIDocument.rootVisualElement.Q(name: "player-2")));
			_playerConnections.Add(2, new PlayerConnectionController(_menuUIDocument.rootVisualElement.Q(name: "player-3")));
			_playerConnections.Add(3, new PlayerConnectionController(_menuUIDocument.rootVisualElement.Q(name: "player-4")));

			foreach(PlayerConnectionController controller in _playerConnections.Values)
			{
				_diContainer.BindInstance(controller);
				_diContainer.Inject(controller);

				controller.PlayerReady += OnPlayerReady;
				controller.PlayerStartRequest += OnPlayerStartRequested;
				controller.PlayerDisconnected += OnPlayerDisconnected;
				controller.BackRequested += OnBack;
			}

			InputDevice latestDevice = GetLastUsedDevice();
			ConnectWithDevice(latestDevice, 0);

			UpdateStartEnabled();
		}

		private static InputDevice GetLastUsedDevice()
		{
			double lastMouseUpdateTime = Mouse.current?.lastUpdateTime ?? -1;
			double lastPCUpdateTime = Math.Max(Keyboard.current?.lastUpdateTime ?? -1, lastMouseUpdateTime);
			double lastGamepadUpdateTime = Gamepad.current?.lastUpdateTime ?? -1;

			if(lastPCUpdateTime > lastGamepadUpdateTime)
			{
				return Keyboard.current;
			}

			return Gamepad.current;
		}

		private void OnDeviceButtonPress(InputControl control)
		{
			if(control.device is not Mouse
				&& !_inputController.DeviceToPlayerLookup.ContainsKey(control.device)
				&& _inputController.TryGetFirstUnusedPlayerId(out int playerId))
			{
				ConnectWithDevice(control.device, playerId);
			}
		}

		private void ConnectWithDevice(InputDevice device, int playerId)
		{
			Debug.Log($"Connecting device {device.displayName} to player {playerId}");
			_inputController.RegisterDevice(playerId, device);
			_playerConnections[playerId].SetConnection(playerId);
			UpdateStartEnabled();
		}

		private void OnPlayerDisconnected(int playerId)
		{
			Debug.Log($"Disconnecting player {playerId}");
			_inputController.UnregisterDevice(playerId);
			_playerConnections[playerId].SetConnection(null);
			UpdateStartEnabled();
		}

		private void OnPlayerReady(int playerId)
		{
			_playerConnections[playerId].SetReady(true);
			UpdateStartEnabled();
		}

		private void OnPlayerStartRequested(int _)
		{
			if(ReadyToStart())
			{
				SceneManager.UnloadSceneAsync("Player Selection Menu");

				foreach(string sceneName in _gameSceneGroup.Scenes)
				{
					SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
				}
			}
		}

		private void UpdateStartEnabled()
		{
			_startBindingVisuals.SetEnabled(ReadyToStart());
		}

		private bool ReadyToStart()
		{
			return _playerConnections.All(kvp => kvp.Value.IsReady);
		}

		private static void OnBack()
		{
			SceneManager.UnloadSceneAsync("Player Selection Menu");
			SceneManager.LoadSceneAsync("Main Menu", LoadSceneMode.Additive);
		}
	}
}