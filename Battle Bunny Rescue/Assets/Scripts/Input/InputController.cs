using Project.Input.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Input
{
	public class InputController
	{
		public IReadOnlyDictionary<int, InputDevice> PlayerToDeviceLookup => _playerToDeviceLookup;
		public IReadOnlyDictionary<InputDevice, int> DeviceToPlayerLookup => _deviceToPlayerLookup;

		private readonly Dictionary<int, InputDevice> _playerToDeviceLookup = new();
		private readonly Dictionary<InputDevice, int> _deviceToPlayerLookup = new();
		private readonly Dictionary<InputAction, Dictionary<int, HashSet<InputCallback>>> _subscribedCallbacks = new();

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
			InputDevice toSwap = null;
			int swappedPlayerId = -1;

			foreach((int playerId, InputDevice boundDevice) in _playerToDeviceLookup)
			{
				if(!boundDevice.added || !boundDevice.enabled)
				{
					toSwap = boundDevice;
					swappedPlayerId = playerId;
					break;
				}
			}

			if(toSwap != null)
			{
				Debug.Log($"Swapped player {swappedPlayerId} from device {toSwap.displayName} with Id {toSwap.deviceId} to new device {device.displayName} with Id {device.deviceId}");
				_playerToDeviceLookup[swappedPlayerId] = device;
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
			_playerToDeviceLookup[playerId] = device;
			_deviceToPlayerLookup[device] = playerId;
		}

		public void UnregisterDeviceForPlayer(int playerId)
		{
			InputDevice removedDevice = _playerToDeviceLookup[playerId];
			_playerToDeviceLookup.Remove(playerId);
			_deviceToPlayerLookup.Remove(removedDevice);
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

			// Player-specific input handling
			if(_deviceToPlayerLookup.TryGetValue(context.control.device, out int playerId))
			{
				if(!callbacks.TryGetValue(playerId, out HashSet<InputCallback> deviceCallbackList))
				{
					return;
				}

				CallCallbacks(deviceCallbackList);
			}

			// Non-player specific input handling
			if(callbacks.TryGetValue(-1, out HashSet<InputCallback> allPlayersCallbackList))
			{
				CallCallbacks(allPlayersCallbackList);
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