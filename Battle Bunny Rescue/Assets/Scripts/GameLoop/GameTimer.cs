using BBR.Events;
using System;
using System.Diagnostics;
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

		private Stopwatch _stopwatch;
		private ProgressBar _progressBar;
		private VisualElement _fill;
		private bool _countdownStarted;

		private void Awake()
		{
			if(_uiDocument == null)
			{
				Debug.LogError($"No {nameof(UIDocument)} found in {nameof(GameTimer)}!");
			}

			_progressBar = _uiDocument.rootVisualElement.Q<ProgressBar>();
			_fill = _progressBar.Q<VisualElement>(className: "unity-progress-bar__progress");

			_stopwatch = new Stopwatch();
			_stopwatch.Start();
		}

		private void Update()
		{
			float remainingPercentage = 1 - (float) Math.Min(_stopwatch.Elapsed.TotalSeconds / _matchLengthSeconds, 1);
			Color color = _gradient.Evaluate(remainingPercentage);

			_progressBar.value = remainingPercentage;
			_fill.style.backgroundColor = color;

			if(!_countdownStarted && _matchLengthSeconds - _stopwatch.Elapsed.TotalSeconds <= 10)
			{
				_countdownStarted = true;
				//EventBus.Fire(new countdo);
			}

			if(_stopwatch.Elapsed.TotalSeconds >= _matchLengthSeconds)
			{
				_stopwatch.Stop();
				enabled = false;
				EventBus.Fire(new GameEndEvent());
			}
		}
	}
}