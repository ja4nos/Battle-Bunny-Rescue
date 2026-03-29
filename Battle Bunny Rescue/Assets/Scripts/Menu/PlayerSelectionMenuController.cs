using Project.Input;
using Project.Utilities;
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

			_playerConnections.Add(0, new PlayerConnectionController());
			_playerConnections.Add(1, new PlayerConnectionController());
			_playerConnections.Add(2, new PlayerConnectionController());
			_playerConnections.Add(3, new PlayerConnectionController());

			foreach(PlayerConnectionController controller in _playerConnections.Values)
			{
				_diContainer.BindInstance(controller);
				_diContainer.Inject(controller);

				controller.PlayerReady += OnPlayerReady;
				controller.PlayerStartRequest += OnPlayerStartRequested;
				controller.PlayerDisconnected += OnPlayerDisconnected;
			}

			OnEnable();

			if(_inputController.DeviceToPlayerLookup.Count > 0)
			{
				foreach((InputDevice device, int playerId) in _inputController.DeviceToPlayerLookup.ToList())
				{
					ConnectWithDevice(device, playerId);
				}
			}
			else
			{
				UpdateStartEnabled();
			}
		}

		private void OnEnable()
		{
			_startBindingVisuals = _menuUIDocument.rootVisualElement.Q(className: "binding", name: "ready-start");

			_playerConnections[0].OnEnable(_menuUIDocument.rootVisualElement.Q(name: "player-1")[0]);
			_playerConnections[1].OnEnable(_menuUIDocument.rootVisualElement.Q(name: "player-2")[0]);
			_playerConnections[2].OnEnable(_menuUIDocument.rootVisualElement.Q(name: "player-3")[0]);
			_playerConnections[3].OnEnable(_menuUIDocument.rootVisualElement.Q(name: "player-4")[0]);
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
			_inputController.RegisterDeviceForPlayer(playerId, device);
			_playerConnections[playerId].SetConnection(playerId);
			UpdateStartEnabled();
		}

		private void OnPlayerDisconnected(int playerId)
		{
			Debug.Log($"Disconnecting player {playerId}");
			_inputController.UnregisterDeviceForPlayer(playerId);
			_playerConnections[playerId].SetConnection(null);
			UpdateStartEnabled();

			// If everyone disconnects, we go back to the menu
			if(_playerConnections.All(kvp => kvp.Value.PlayerId == null))
			{
				BackToMenu();
			}
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
				SceneManager.UnloadSceneAsync("Menu Environment");

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
			int readyCount = 0;
			int connectedCount = 0;

			foreach(PlayerConnectionController controller in _playerConnections.Values)
			{
				if(controller.PlayerId.HasValue)
				{
					connectedCount++;
				}

				if(controller.IsReady)
				{
					readyCount++;
				}
			}

			return connectedCount > 0 && readyCount == connectedCount;
		}

		private void BackToMenu()
		{
			Debug.Log("Back");

			foreach(PlayerConnectionController controller in _playerConnections.Values)
			{
				if(controller.PlayerId.HasValue)
				{
					_inputController.UnregisterDeviceForPlayer(controller.PlayerId.Value);
				}
			}

			SceneManager.UnloadSceneAsync("Player Selection Menu");
			SceneManager.LoadSceneAsync("Main Menu", LoadSceneMode.Additive);
		}

		private void OnDestroy()
		{
			foreach(PlayerConnectionController controller in _playerConnections.Values)
			{
				controller.Dispose();
			}
		}
	}
}