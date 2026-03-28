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
        private readonly Dictionary<InputAction, Dictionary<int, HashSet<InputCallback>>> _subscribedCallbacks = new();
        private readonly Dictionary<InputDevice, int> _deviceToPlayerLookup = new();

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

            foreach((InputDevice boundDevice, int playerId) in _deviceToPlayerLookup)
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
                _deviceToPlayerLookup.Remove(toSwap);
                _deviceToPlayerLookup[device] = swappedPlayerId;
            }
        }

        public void RegisterDevice(int playerId, int deviceId)
        {
            InputDevice device = InputSystem.devices.FirstOrDefault(dev => dev.deviceId.Equals(deviceId));

            if(device == null)
            {
                Debug.LogError($"No device with Id '{deviceId}' found!");
                return;
            }
            
            _deviceToPlayerLookup[device] = playerId;
        }

        public void UnregisterDevice(int playerId)
        {
            InputDevice toRemove = null;
            
            foreach((InputDevice device, int player) in _deviceToPlayerLookup)
            {
                if(player.Equals(playerId))
                {
                    toRemove = device;
                    break;
                }
            }

            if(toRemove == null)
            {
                Debug.LogError($"No device was found for player '{playerId}'!");
                return;
            }

            _deviceToPlayerLookup.Remove(toRemove);
        }
        
        public void SubscribeAction(string actionName, InputCallback inputCallback)
        {
            SubscribeAction(actionName, null, inputCallback);
        }
        
        public void SubscribeAction(string actionName, string actionMapName, InputCallback inputCallback)
        {
            InputAction action = actionMapName == null
                ? InputSystem.actions.FindAction(actionName)
                : InputSystem.actions.FindActionMap(actionMapName).FindAction(actionName);

            if(action == null)
            {
                Debug.LogError($"Action {actionName}{(actionMapName == null ? "" : $" in map {actionMapName}")} not found!");
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

            if(!callbacks.TryGetValue(inputCallback.PlayerId, out HashSet<InputCallback> deviceCallbackList))
            {
                callbacks[inputCallback.PlayerId] = deviceCallbackList = new HashSet<InputCallback>();
            }
            
            deviceCallbackList.Add(inputCallback);
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

            if(!_subscribedCallbacks.TryGetValue(action, out Dictionary<int, HashSet<InputCallback>> callbacks))
            {
                Debug.LogWarning($"Action {actionName}{(actionMapName == null ? "" : $" in map {actionMapName}")} was never subscribed to!");
                return;
            }

            if(!callbacks.TryGetValue(inputCallback.PlayerId, out HashSet<InputCallback> deviceCallbackList))
            {
                Debug.LogWarning($"Action {actionName}{(actionMapName == null ? "" : $" in map {actionMapName}")} was never subscribed to for player {inputCallback.PlayerId}!");
                return;
            }

            deviceCallbackList.Remove(inputCallback);

            if(deviceCallbackList.Count == 0)
            {
                callbacks.Remove(inputCallback.PlayerId);

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
            
            if(!_deviceToPlayerLookup.TryGetValue(context.control.device, out int playerId))
            {
                Debug.LogWarning($"Input coming from device {context.control.device.displayName} with Id {context.control.device.displayName} is not assigned to any player!");
                return;
            }
            
            if(!callbacks.TryGetValue(playerId, out HashSet<InputCallback> deviceCallbackList))
            {
                Debug.LogError($"Player with Id {playerId} and name '{context.control.device.displayName}' with action {context.action} not found inside subscribed callbacks!");
                return;
            }

            foreach(InputCallback inputCallback in deviceCallbackList)
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