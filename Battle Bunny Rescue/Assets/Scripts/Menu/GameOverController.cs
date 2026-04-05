using BBR.AudioPlayer;
using BBR.CameraController;
using BBR.Events;
using BBR.GameLoop;
using BBR.Movement;
using DG.Tweening;
using Project.Input;
using Project.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Zenject;

namespace Project.Menu
{
	public class GameOverController : MonoBehaviour
	{
		private static readonly List<string> _podiumClasses = new()
		{
			"first",
			"second",
			"third"
		};

		[SerializeField] private UIDocument _uiDocument;
		[SerializeField] private SceneGroup _gameSceneGroup;
		[SerializeField] private SceneGroup _mainMenuSceneGroup;
		[SerializeField] private GameObject _playerVisualsPrefab;
		[SerializeField] private AnimationCurve _jumpAnimationCurve;
		[SerializeField] private AudioHolder _clickSfx;

		[Inject] private InputController _inputController;

		private List<PlayerVisualsRenderer> _playerRenderers;
		private VisualElement _gameOverUI;
		private Button _exitButton;
		private bool _shown;

		private void Awake()
		{
			if(_uiDocument == null)
			{
				Debug.LogError($"No pause menu UI document has been assigned to {nameof(PauseMenuController)}!", this);
				return;
			}

			_shown = false;
			EventBus.Register<GameEndEvent>(OnGameEnd);

			_playerRenderers = new List<PlayerVisualsRenderer>();
			for(int i = 0; i < 4; i++)
			{
				_playerRenderers.Add(new PlayerVisualsRenderer(_playerVisualsPrefab, transform, i, spin: false));
			}
		}

		private void OnEnable()
		{
			_gameOverUI = _uiDocument.rootVisualElement.Q<VisualElement>(name: "game-over");
			_exitButton = _uiDocument.rootVisualElement.Q<Button>(name: "exit-button");
			_exitButton.clicked += OnExitClicked;

			SetUIShown(_shown);

			for(int i = 0; i < _playerRenderers.Count; i++)
			{
				PlayerVisualsRenderer playerVisualsRenderer = _playerRenderers[i];
				playerVisualsRenderer.OnEnable(_gameOverUI.Q<VisualElement>($"player-{i}")[0]);
			}
		}

		private void OnGameEnd(GameEndEvent _)
		{
			SetUIShown(true);
		}

		private void SetUIShown(bool shown)
		{
			_shown = shown;

			_uiDocument.rootVisualElement.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
			_gameOverUI.EnableInClassList("shown", shown);

			if(shown)
			{
				SetupScreen();
			}
		}

		private static T[] DestroyComponents<T>() where T : Component
		{
			T[] components = FindObjectsByType<T>();

			foreach(T component in components)
			{
				Destroy(component);
			}

			return components;
		}

		private void SetupScreen()
		{
			DestroyComponents<PlayerCinemachineInputProvider>();
			DestroyComponents<BunnyMovementPlayer>();
			BunnyPlayer[] players = DestroyComponents<BunnyPlayer>();

			List<IGrouping<int, (int PlayerId, int SavedBunniesCount)>> playerScores = players
				.Select(bp => (bp.PlayerId, bp.SavedBunniesCount + bp.CapturedBunniesCount))
				.GroupBy(kvp => kvp.Item2)
				.OrderByDescending(group => group.Key)
				.ToList();

			for(int i = 0; i < _playerRenderers.Count; i++)
			{
				PlayerVisualsRenderer playerVisualsRenderer = _playerRenderers[i];
				playerVisualsRenderer.RootElement.SetEnabled(_inputController.PlayerToDeviceLookup.ContainsKey(i));
			}

			int c = 0;
			foreach(IGrouping<int, (int playerId, int savedBunniesCount)> grouping in playerScores)
			{
				foreach((int playerId, int savedBunniesCount) in grouping)
				{
					PlayerVisualsRenderer playerVisualsRenderer = _playerRenderers[playerId];
					Label scoreLabel = playerVisualsRenderer.RootElement.Q<Label>(name: "score-text");
					scoreLabel.text = $"x{savedBunniesCount}";

					if(_podiumClasses.Count > c)
					{
						playerVisualsRenderer.RootElement.AddToClassList(_podiumClasses[c]);
					}

					if(c == 0)
					{
						DOTween.To
							(
								getter: () => playerVisualsRenderer.RootElement.resolvedStyle.translate.y,
								setter: y => playerVisualsRenderer.RootElement.style.translate = new StyleTranslate(new Translate(0, y)),
								endValue: -35f,
								duration: 0.51f
							)
							.SetEase(_jumpAnimationCurve)
							.SetLoops(-1)
							.SetId(playerVisualsRenderer);
					}
				}

				c++;
			}

			_exitButton.Focus();
		}

		private void OnExitClicked()
		{
			_clickSfx.Play();

			foreach(string sceneName in _gameSceneGroup.Scenes)
			{
				SceneManager.UnloadSceneAsync(sceneName);
			}

			foreach(string sceneName in _mainMenuSceneGroup.Scenes)
			{
				SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			}

			PlayerHelper.ClearPlayerColors();
		}

		private void OnDestroy()
		{
			foreach(PlayerVisualsRenderer playerVisualsRenderer in _playerRenderers)
			{
				DOTween.Kill(playerVisualsRenderer);
			}

			EventBus.Unregister<GameEndEvent>(OnGameEnd);
		}
	}
}