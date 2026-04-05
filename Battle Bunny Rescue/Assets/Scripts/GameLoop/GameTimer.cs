using BBR.Events;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace BBR.GameLoop
{
	public class GameTimer : MonoBehaviour
	{
		[SerializeField] private UIDocument _uiDocument;
		[SerializeField] private double _matchLengthSeconds;
		[SerializeField] private Gradient _gradient;

		private ProgressBar _progressBar;
		private VisualElement _fill;
		private float _secondsElapsed;
		private bool _countdownStarted;

		private void Awake()
		{
			if(_uiDocument == null)
			{
				Debug.LogError($"No {nameof(UIDocument)} found in {nameof(GameTimer)}!");
			}

			_progressBar = _uiDocument.rootVisualElement.Q<ProgressBar>();
			_fill = _progressBar.Q<VisualElement>(className: "unity-progress-bar__progress");
		}

		private void Update()
		{
			_secondsElapsed += Time.deltaTime;

			float remainingPercentage = 1 - (float) Math.Min(_secondsElapsed / _matchLengthSeconds, 1);
			Color color = _gradient.Evaluate(remainingPercentage);

			_progressBar.value = remainingPercentage;
			_fill.style.backgroundColor = color;

			if(!_countdownStarted && _matchLengthSeconds - _secondsElapsed <= 10)
			{
				Debug.Log("Final countdown!!!");
				_countdownStarted = true;
				EventBus.Fire(new StartCountdownEvent());
			}

			if(_secondsElapsed >= _matchLengthSeconds)
			{
				enabled = false;
				EventBus.Fire(new GameEndEvent());
			}
		}
	}
}