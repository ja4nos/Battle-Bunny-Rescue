using BBR.Events;
using BBR.GameLoop;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Menu
{
	public class PlayerUIController : IDisposable
	{
		private readonly int _playerId;

		private VisualElement _panel;
		private VisualElement _score;
		private Label _scoreLabel;
		private List<Image> _basketBunnies;

		private int _savedBunniesCount;

		public PlayerUIController(int playerId)
		{
			_playerId = playerId;
			EventBus.Register<SavedBunniesEvent>(OnSavedBunniesChanged);
			EventBus.Register<CapturedBunniesEvent>(OnCapturedBunniesChanged);
		}

		public void OnEnable(VisualElement root)
		{
			_panel = root.Q<VisualElement>(name: "panel");
			_score = root.Q<VisualElement>(name: "score");
			_scoreLabel = root.Q<Label>(name: "count");
			VisualElement basket = root.Q(name: "basket");
			_basketBunnies = basket.Query<Image>().Build().ToList();

			Color color = PlayerHelper.GetPlayerColor(_playerId);

			SetBorderColor(_panel, color);
			SetBorderColor(basket, color);
		}

		private static void SetBorderColor(VisualElement element, Color color)
		{
			element.style.borderBottomColor = new StyleColor(color);
			element.style.borderLeftColor = new StyleColor(color);
			element.style.borderTopColor = new StyleColor(color);
			element.style.borderRightColor = new StyleColor(color);
		}

		private void OnSavedBunniesChanged(SavedBunniesEvent evt)
		{
			if(_scoreLabel != null && _playerId == evt.PlayerId && _savedBunniesCount != evt.SavedBunniesCount)
			{
				_savedBunniesCount = evt.SavedBunniesCount;
				_scoreLabel.text = $"x{evt.SavedBunniesCount}";

				DOTween.Kill(_score);
				DOTween.To(() => _score.resolvedStyle.scale.value.x, scale => _score.style.scale = new StyleScale(new Vector2(scale, scale)), 1.5f, 0.1f)
					.SetEase(Ease.Linear)
					.SetLoops(2, LoopType.Yoyo)
					.SetId(_score)
					.SetAutoKill();
			}
		}

		private void OnCapturedBunniesChanged(CapturedBunniesEvent evt)
		{
			if(_basketBunnies != null && _playerId == evt.PlayerId)
			{
				for(int i = 0; i < _basketBunnies.Count; i++)
				{
					Image basketBunny = _basketBunnies[i];
					bool captured = i < evt.CapturedBunniesCount;
					basketBunny.EnableInClassList("captured", captured);
				}
			}
		}

		public void Dispose()
		{
			EventBus.Unregister<SavedBunniesEvent>(OnSavedBunniesChanged);
			EventBus.Unregister<CapturedBunniesEvent>(OnCapturedBunniesChanged);
		}
	}
}