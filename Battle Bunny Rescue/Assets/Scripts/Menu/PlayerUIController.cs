using BBR.Events;
using BBR.GameLoop;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Menu
{
	public class PlayerUIController : IDisposable
	{
		private readonly int _playerId;

		private VisualElement _panel;
		private Label _scoreLabel;

		public PlayerUIController(int playerId)
		{
			_playerId = playerId;
			EventBus.Register<SavedBunniesEvent>(OnSavedBunniesChanged);
		}

		public void OnEnable(VisualElement root)
		{
			_panel = root.Q<VisualElement>(name: "panel");
			_scoreLabel = root.Q<Label>(name: "count");

			Color color = PlayerHelper.GetPlayerColor(_playerId);
			_panel.style.borderBottomColor = new StyleColor(color);
			_panel.style.borderLeftColor = new StyleColor(color);
			_panel.style.borderTopColor = new StyleColor(color);
			_panel.style.borderRightColor = new StyleColor(color);
		}

		private void OnSavedBunniesChanged(SavedBunniesEvent evt)
		{
			if(_scoreLabel != null && _playerId == evt.PlayerId)
			{
				_scoreLabel.text = $"x{evt.SavedBunniesCount}";
			}
		}

		public void Dispose()
		{
			EventBus.Unregister<SavedBunniesEvent>(OnSavedBunniesChanged);
		}
	}
}