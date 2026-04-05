using BBR.AudioPlayer;
using BBR.GameLoop;
using Project.Input;
using Project.Input.Models;
using Project.Utilities;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Zenject;
using Cursor = UnityEngine.Cursor;

namespace Project.Menu
{
	public class PauseMenuController : MonoBehaviour
	{
		[SerializeField] private UIDocument _menuUIDocument;
		[SerializeField] private SceneGroup _gameSceneGroup;
		[SerializeField] private SceneGroup _mainMenuSceneGroup;
		[SerializeField] private AudioHolder _clickSfx;

		[Inject] private InputController _inputController;

		private InputCallback _inputCallback;
		private Button _resumeButton;
		private bool _shown;

		private void Awake()
		{
			if(_menuUIDocument == null)
			{
				Debug.LogError($"No pause menu UI document has been assigned to {nameof(PauseMenuController)}!", this);
				Destroy(this);
				return;
			}

			_inputCallback = new InputCallback { PlayerId = null, PerformedCallback = _ => ToggleUIShown() };
			_inputController.SubscribeAction("Cancel", "UI", _inputCallback);
			_shown = false;
		}

		private void OnEnable()
		{
			_resumeButton = _menuUIDocument.rootVisualElement.Q<Button>(name: "resume-button");
			Button optionsButton = _menuUIDocument.rootVisualElement.Q<Button>(name: "options-button");
			Button exitButton = _menuUIDocument.rootVisualElement.Q<Button>(name: "exit-button");

			_resumeButton.clicked += OnResumeClicked;
			optionsButton.clicked += OnOptionsClicked;
			exitButton.clicked += OnExitClicked;

			_menuUIDocument.rootVisualElement.RegisterCallback<FocusEvent>(_ => { _clickSfx.Play(); }, TrickleDown.TrickleDown);
			_menuUIDocument.rootVisualElement.RegisterCallback<NavigationSubmitEvent>(_ => { _clickSfx.Play(); }, TrickleDown.TrickleDown);

			SetUIShown(_shown);
		}

		private void OnResumeClicked()
		{
			SetUIShown(false);
		}

		private void ToggleUIShown()
		{
			SetUIShown(!_shown);
		}

		private void SetUIShown(bool shown)
		{
			_shown = shown;
			_menuUIDocument.rootVisualElement.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
			Time.timeScale = shown ? 0 : 1;

			if(shown)
			{
				_resumeButton.Focus();
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
			else
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}

		private static void OnOptionsClicked()
		{
			throw new NotImplementedException();
		}

		private void OnExitClicked()
		{
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
			_inputController.UnsubscribeAction("Cancel", "UI", _inputCallback);
			Time.timeScale = 1;
		}
	}
}
