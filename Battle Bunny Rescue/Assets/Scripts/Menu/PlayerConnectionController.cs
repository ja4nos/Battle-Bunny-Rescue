using Project.Input;
using Project.Input.Models;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

namespace Project.Menu
{
	public class PlayerConnectionController
	{
		public event Action<int> PlayerReady;
		public event Action<int> PlayerStartRequest;
		public event Action<int> PlayerDisconnected;
		public event Action BackRequested;

		public bool IsReady => _readied || !_playerId.HasValue;

		[Inject] private InputController _inputController;

		private readonly VisualElement _root;
		private readonly Label _connectInfoLabel;

		private int? _playerId;
		private bool _readied;

		public PlayerConnectionController(VisualElement root)
		{
			_root = root;
			_connectInfoLabel = root.Q<Label>(name: "connect-info");
		}

		public void SetConnection(int? playerId)
		{
			_playerId = playerId;
			_connectInfoLabel.style.display = playerId.HasValue ? DisplayStyle.None : DisplayStyle.Flex;

			if(playerId.HasValue)
			{
				_inputController.SubscribeAction("Disconnect", "UI", new InputCallback { PlayerId = playerId, PerformedCallback = OnDisconnect });
				_inputController.SubscribeAction("Ready", "UI", new InputCallback { PlayerId = playerId, PerformedCallback = OnReady });
			}

			SetReady(false);
		}

		public void SetReady(bool ready)
		{
			_readied = ready;
			_root.EnableInClassList("ready", ready);
		}

		private void OnReady(InputAction.CallbackContext _)
		{
			if(_playerId.HasValue)
			{
				if(!_readied)
				{
					PlayerReady?.Invoke(_playerId.Value);
				}
				else
				{
					PlayerStartRequest?.Invoke(_playerId.Value);
				}
			}
		}

		private void OnDisconnect(InputAction.CallbackContext _)
		{
			if(_playerId.HasValue)
			{
				if(_playerId == 0)
				{
					BackRequested?.Invoke();
				}
				else
				{
					PlayerDisconnected?.Invoke(_playerId.Value);
				}
			}
		}
	}
}