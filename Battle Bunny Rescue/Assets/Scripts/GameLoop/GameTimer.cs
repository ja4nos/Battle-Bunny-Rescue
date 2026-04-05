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
		[SerializeField] private Color _startColor;
		[SerializeField] private Color _endColor;

		private Stopwatch _stopwatch;
		private ProgressBar _progressBar;
		private VisualElement _fill;

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
			float elapsedPercentage = (float) Math.Min(_stopwatch.Elapsed.TotalSeconds / _matchLengthSeconds, 1);
			Color color = Color.Lerp(_startColor, _endColor, elapsedPercentage);

			_progressBar.value = 1 - elapsedPercentage;
			_fill.style.backgroundColor = color;

			if(_stopwatch.Elapsed.TotalSeconds >= _matchLengthSeconds)
			{
				_stopwatch.Stop();
				enabled = false;
				EventBus.Fire(new GameEndEvent());
			}
		}
	}
}