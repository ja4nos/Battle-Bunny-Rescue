using Project.Input;
using Project.Input.Models;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

namespace Project.Menu
{
	public class PlayerConnectionController : IDisposable
	{
		public event Action<int> PlayerReady;
		public event Action<int> PlayerStartRequest;
		public event Action<int> PlayerDisconnected;

		public int? PlayerId { get; private set; }
		public bool IsReady { get; private set; }

		[Inject] private InputController _inputController;

		private VisualElement _root;
		private Label _readiedLabel;

		private InputCallback _disconnectCallback;
		private InputCallback _readyCallback;

		public PlayerConnectionController()
		{
			_disconnectCallback = new InputCallback { PerformedCallback = OnDisconnect };
			_readyCallback = new InputCallback { PerformedCallback = OnReady };
		}

		public void OnEnable(VisualElement root)
		{
			_root = root;
			_readiedLabel = root.Q<Label>(name: "readied");
		}

		public void SetConnection(int? playerId)
		{
			if(PlayerId != playerId)
			{
				PlayerId = playerId;

				if(playerId.HasValue)
				{
					_root.AddToClassList("connected");

					_disconnectCallback.PlayerId = playerId.Value;
					_readyCallback.PlayerId = playerId.Value;

					_inputController.SubscribeAction("Disconnect", "UI", _disconnectCallback);
					_inputController.SubscribeAction("Ready", "UI", _readyCallback);
				}
				else
				{
					_root.RemoveFromClassList("connected");

					_inputController.UnsubscribeAction("Disconnect", "UI", _disconnectCallback);
					_inputController.UnsubscribeAction("Ready", "UI", _readyCallback);
				}

				SetReady(false);
			}
		}

		public void SetReady(bool ready)
		{
			IsReady = ready;
			_root.EnableInClassList("ready", ready);
			_readiedLabel.text = ready ? "Ready" : "Not Ready";
		}

		private void OnReady(InputAction.CallbackContext _)
		{
			if(PlayerId.HasValue)
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
					PlayerDisconnected?.Invoke(PlayerId.Value);
				}
			});
		}

		public void Dispose()
		{
			_inputController?.UnsubscribeAction("Disconnect", "UI", _disconnectCallback);
			_inputController?.UnsubscribeAction("Ready", "UI", _readyCallback);
		}
	}
}