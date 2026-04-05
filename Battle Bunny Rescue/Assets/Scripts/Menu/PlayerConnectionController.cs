using Project.Input;
using Project.Input.Models;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

namespace Project.Menu
{
	public class PlayerConnectionController : IDisposable
	{
		public event Action<int> PlayerNotReady;
		public event Action<int> PlayerReady;
		public event Action<int> PlayerStartRequest;
		public event Action<int> PlayerDisconnected;

		public int? PlayerId { get; private set; }
		public Color PlayerColor => _playerVisualsRenderer.PlayerColor;
		public bool IsReady { get; private set; }

		[Inject] private InputController _inputController;

		private readonly PlayerVisualsRenderer _playerVisualsRenderer;

		private float _connectedTime;
		private VisualElement _root;
		private Label _readiedLabel;

		private InputCallback _disconnectCallback;
		private InputCallback _readyCallback;
		private InputCallback _navigateCallback;

		public PlayerConnectionController(GameObject playerVisualsPrefab, Transform parentTransform, int playerId)
		{
			_playerVisualsRenderer = new PlayerVisualsRenderer(playerVisualsPrefab, parentTransform, playerId, spin: true);
			_disconnectCallback = new InputCallback { PerformedCallback = OnDisconnect };
			_readyCallback = new InputCallback { PerformedCallback = OnReady };
			_navigateCallback = new InputCallback { StartedCallback = OnNavigate, PerformedCallback = OnNavigate };
		}

		private void OnNavigate(InputAction.CallbackContext obj)
		{
			Vector2 value = obj.ReadValue<Vector2>();

			if(Math.Abs(value.x) > Math.Abs(value.y))
			{
				if(value.x > 0)
				{
					_playerVisualsRenderer.SetNextAvailablePlayerColor();
				}
				else
				{
					_playerVisualsRenderer.SetPreviousAvailablePlayerColor();
				}
			}
		}

		public void OnEnable(VisualElement root)
		{
			_root = root;
			_readiedLabel = root.Q<Label>(name: "readied");
			_playerVisualsRenderer.OnEnable(root);
		}

		public void SetConnection(int? playerId)
		{
			if(PlayerId != playerId)
			{
				PlayerId = playerId;

				if(playerId.HasValue)
				{
					_connectedTime = Time.time;
					_root.AddToClassList("connected");

					_disconnectCallback.PlayerId = playerId.Value;
					_readyCallback.PlayerId = playerId.Value;
					_navigateCallback.PlayerId = playerId.Value;

					_inputController.SubscribeAction("Disconnect", "UI", _disconnectCallback);
					_inputController.SubscribeAction("Ready", "UI", _readyCallback);
					_inputController.SubscribeAction("Navigate", "UI", _navigateCallback);
				}
				else
				{
					_root.RemoveFromClassList("connected");

					_inputController.UnsubscribeAction("Disconnect", "UI", _disconnectCallback);
					_inputController.UnsubscribeAction("Ready", "UI", _readyCallback);
					_inputController.UnsubscribeAction("Navigate", "UI", _navigateCallback);
				}

				SetReady(false);
			}
		}

		public void SetReady(bool ready)
		{
			IsReady = ready;
			_root.EnableInClassList("ready", ready);
			_readiedLabel.text = ready ? "Ready" : "Not Ready";
			_playerVisualsRenderer.SetReady(ready);
		}

		private void OnReady(InputAction.CallbackContext _)
		{
			const float debounceTime = 0.2f;

			if(PlayerId.HasValue && Time.time - _connectedTime > debounceTime)
			{
				if(!IsReady)
				{
					PlayerReady?.Invoke(PlayerId.Value);
				}
				else
				{
					PlayerStartRequest?.Invoke(PlayerId.Value);
				}
			}
		}

		private void OnDisconnect(InputAction.CallbackContext _)
		{
			_root.schedule.Execute(() =>
			{
				if(PlayerId.HasValue)
				{
					if(IsReady)
					{
						PlayerNotReady?.Invoke(PlayerId.Value);
					}
					else
					{
						PlayerDisconnected?.Invoke(PlayerId.Value);
					}
				}
			});
		}

		public void Dispose()
		{
			_inputController?.UnsubscribeAction("Disconnect", "UI", _disconnectCallback);
			_inputController?.UnsubscribeAction("Ready", "UI", _readyCallback);
			_playerVisualsRenderer.Dispose();
		}
	}
}