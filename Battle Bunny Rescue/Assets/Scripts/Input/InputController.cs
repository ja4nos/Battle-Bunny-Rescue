using Project.Input.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Project.Input
{
	public class InputController
	{
		public IReadOnlyDictionary<int, InputDevice[]> PlayerToDeviceLookup => _playerToDeviceLookup;
		public IReadOnlyDictionary<InputDevice, int> DeviceToPlayerLookup => _deviceToPlayerLookup;

		private readonly Dictionary<int, InputDevice[]> _playerToDeviceLookup = new();
		private readonly Dictionary<InputDevice, int> _deviceToPlayerLookup = new();
		private readonly Dictionary<InputAction, Dictionary<int, HashSet<InputCallback>>> _subscribedCallbacks = new();
		private readonly Dictionary<int, Dictionary<string, InputActionMap>> _playerActionMaps = new();

		public InputController()
		{
			InputSystem.onDeviceChange += OnDeviceChanged;
		}

		private void OnDeviceChanged(InputDevice device, InputDeviceChange changeEvent)
		{
			if(changeEvent is InputDeviceChange.Added or InputDeviceChange.Enabled or InputDeviceChange.UsageChanged or InputDeviceChange.ConfigurationChanged)
			{
				HandleNewDevice(device);
			}
		}

		private void HandleNewDevice(InputDevice device)
		{
			InputDevice[] toSwap = null;
			int swappedPlayerId = -1;

			foreach((int playerId, InputDevice[] boundDevices) in _playerToDeviceLookup)
			{
				foreach(InputDevice boundDevice in boundDevices)
				{
					if(!boundDevice.added || !boundDevice.enabled)
					{
						toSwap = boundDevices;
						swappedPlayerId = playerId;
						break;
					}
				}
			}

			if(toSwap != null)
			{
				Debug.Log($"Swapped player {swappedPlayerId} from device(s) {string.Join(", ", toSwap.Select(d => $"{d.displayName} ({d.deviceId})"))} to new device {device.displayName} ({device.deviceId})");
				_playerToDeviceLookup[swappedPlayerId] = new[] { device };
				_deviceToPlayerLookup[device] = swappedPlayerId;
			}
		}

		public bool TryGetFirstUnusedPlayerId(out int playerId)
		{
			for(int i = 0; i < 4; ++i)
			{
				if(!_playerToDeviceLookup.ContainsKey(i))
				{
					playerId = i;
					return true;
				}
			}

			playerId = -1;
			return false;
		}

		public void RegisterDeviceForPlayer(int playerId, int deviceId)
		{
			InputDevice device = InputSystem.devices.FirstOrDefault(dev => dev.deviceId.Equals(deviceId));

			if(device == null)
			{
				Debug.LogError($"No device with Id '{deviceId}' found!");
				return;
			}

			RegisterDeviceForPlayer(playerId, device);
		}

		public void RegisterDeviceForPlayer(int playerId, InputDevice device)
		{
			_deviceToPlayerLookup[device] = playerId;

			InputDevice[] devices;

			if(device is Keyboard && Mouse.current != null)
			{
				devices = new[] { device, Mouse.current };
				_deviceToPlayerLookup[Mouse.current] = playerId;
			}
			else if(device is Mouse)
			{
				return;
			}
			else
			{
				devices = new[] { device };
			}

			_playerToDeviceLookup[playerId] = devices;

			foreach(InputActionMap sourceMap in InputSystem.actions.actionMaps)
			{
				InputActionMap playerMap = sourceMap.Clone();

				playerMap.devices = new ReadOnlyArray<InputDevice>(devices);
				playerMap.Enable();

				if(!_playerActionMaps.TryGetValue(playerId, out Dictionary<string, InputActionMap> maps))
				{
					_playerActionMaps[playerId] = maps = new Dictionary<string, InputActionMap>();
				}

				maps[playerMap.name] = playerMap;
			}
		}

		public void UnregisterDeviceForPlayer(int playerId)
		{
			if(_playerActionMaps.TryGetValue(playerId, out Dictionary<string, InputActionMap> actionMaps))
			{
				foreach(InputActionMap map in actionMaps.Values)
				{
					map.Disable();
					map.Dispose();
				}

				_playerActionMaps.Remove(playerId);
			}

			foreach(InputDevice removedDevice in _playerToDeviceLookup[playerId])
			{
				_deviceToPlayerLookup.Remove(removedDevice);
			}

			_playerToDeviceLookup.Remove(playerId);
		}

		public void SubscribeAction(string actionName, InputCallback inputCallback)
		{
			SubscribeAction(actionName, null, inputCallback);
		}

		public void SubscribeAction(string actionName, string actionMapName, InputCallback inputCallback)
		{
			if(!TryGetAction(actionName, actionMapName, out InputAction action))
			{
				return;
			}

			if(!_subscribedCallbacks.TryGetValue(action, out Dictionary<int, HashSet<InputCallback>> callbacks))
			{
				_subscribedCallbacks[action] = callbacks = new Dictionary<int, HashSet<InputCallback>>();

				// Only subscribe the first time, when no one has done so yet
				action.started += ActionCallback;
				action.performed += ActionCallback;
				action.canceled += ActionCallback;
				action.Enable();
			}

			if(!callbacks.TryGetValue(inputCallback.PlayerId ?? -1, out HashSet<InputCallback> deviceCallbackList))
			{
				callbacks[inputCallback.PlayerId ?? -1] = deviceCallbackList = new HashSet<InputCallback>();
			}

			deviceCallbackList.Add(inputCallback);
		}

		public static bool TryGetAction(string actionName, string actionMapName, out InputAction action)
		{
			action = actionMapName == null
				? InputSystem.actions.FindAction(actionName)
				: InputSystem.actions.FindActionMap(actionMapName).FindAction(actionName);

			if(action == null)
			{
				Debug.LogError($"Action {actionName}{(actionMapName == null ? "" : $" in map {actionMapName}")} not found!");
				return false;
			}

			return true;
		}

		public bool TryReadValue<T>(string actionName, string actionMapName, int playerId, out T value) where T : struct
		{
			value = default;

			if(!_playerActionMaps.TryGetValue(playerId, out Dictionary<string, InputActionMap> maps))
			{
				Debug.LogError($"No action maps found for player {playerId}!");
				return false;
			}

			if(!maps.TryGetValue(actionMapName, out InputActionMap map))
			{
				Debug.LogError($"No action map found for player {playerId} named {actionMapName}!");
				return false;
			}

			InputAction action = map.FindAction(actionName);
			if(action == null)
			{
				Debug.LogError($"Action {actionName} not found!");
				return false;
			}

			value = action.ReadValue<T>();
			return true;
		}

		public void UnsubscribeAction(string actionName, InputCallback inputCallback)
		{
			UnsubscribeAction(actionName, null, inputCallback);
		}

		public void UnsubscribeAction(string actionName, string actionMapName, InputCallback inputCallback)
		{
			InputAction action = actionMapName == null
				? InputSystem.actions.FindAction(actionName)
				: InputSystem.actions.FindActionMap(actionMapName).FindAction(actionName);

			if(action == null)
			{
				Debug.LogError($"Action {actionName}{(actionMapName == null ? "" : $" in map {actionMapName}")} not found!");
				return;
			}

			if(!_subscribedCallbacks.TryGetValue(action, out Dictionary<int, HashSet<InputCallback>> callbacks)
				|| !callbacks.TryGetValue(inputCallback.PlayerId ?? -1, out HashSet<InputCallback> deviceCallbackList))
			{
				return;
			}

			deviceCallbackList.Remove(inputCallback);

			if(deviceCallbackList.Count == 0)
			{
				callbacks.Remove(inputCallback.PlayerId ?? -1);

				if(callbacks.Count == 0)
				{
					_subscribedCallbacks.Remove(action);

					// Only unsubscribe when there's no listeners anymore
					action.started -= ActionCallback;
					action.performed -= ActionCallback;
					action.canceled -= ActionCallback;
					action.Disable();
				}
			}
		}

		private void ActionCallback(InputAction.CallbackContext context)
		{
			if(!_subscribedCallbacks.TryGetValue(context.action, out Dictionary<int, HashSet<InputCallback>> callbacks))
			{
				Debug.LogError($"Action {context.action} not found inside subscribed callbacks!");
				return;
			}

			// Non-player specific input handling
			if(callbacks.TryGetValue(-1, out HashSet<InputCallback> allPlayersCallbackList))
			{
				CallCallbacks(allPlayersCallbackList);
			}

			// Player-specific input handling
			if(_deviceToPlayerLookup.TryGetValue(context.control.device, out int playerId))
			{
				if(callbacks.TryGetValue(playerId, out HashSet<InputCallback> deviceCallbackList))
				{
					CallCallbacks(deviceCallbackList);
				}
			}

			return;

			void CallCallbacks(IEnumerable<InputCallback> callbackCollection)
			{
				foreach(InputCallback inputCallback in callbackCollection)
				{
					Action<InputAction.CallbackContext> callback = context.phase switch
					{
						InputActionPhase.Started => inputCallback.StartedCallback,
						InputActionPhase.Performed => inputCallback.PerformedCallback,
						InputActionPhase.Canceled => inputCallback.CanceledCallback,
						_ => throw new ArgumentOutOfRangeException()
					};

					callback?.Invoke(context);
				}
			}
		}
	}
}
