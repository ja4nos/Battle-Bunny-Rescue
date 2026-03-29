using Project.Input;
using Project.Input.Models;
using Project.Utilities;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Zenject;

namespace Project.Menu
{
	public class PauseMenuController : MonoBehaviour
	{
		[SerializeField] private UIDocument _menuUIDocument;
		private SceneGroup _gameSceneGroup;

		[Inject] private InputController _inputController;

		private InputCallback _inputCallback;

		private void Awake()
		{
			if(_menuUIDocument == null)
			{
				Debug.LogError($"No pause menu UI document has been assigned to {nameof(PauseMenuController)}!", this);
				return;
			}

			Button resumeButton = _menuUIDocument.rootVisualElement.Q<Button>(name: "resume-button");
			Button optionsButton = _menuUIDocument.rootVisualElement.Q<Button>(name: "options-button");
			Button exitButton = _menuUIDocument.rootVisualElement.Q<Button>(name: "exit-button");

			resumeButton.clicked += OnResumeClicked;
			optionsButton.clicked += OnOptionsClicked;
			exitButton.clicked += OnExitClicked;

			_inputCallback = new InputCallback { PlayerId = null, PerformedCallback = _ => ToggleUIShown() };
			_inputController.SubscribeAction("Cancel", "UI", _inputCallback);

			SetUIShown(false);
		}

		private void OnResumeClicked()
		{
			SetUIShown(false);
		}

		private void ToggleUIShown()
		{
			SetUIShown(_menuUIDocument.rootVisualElement.resolvedStyle.display == DisplayStyle.None);
		}

		private void SetUIShown(bool shown)
		{
			_menuUIDocument.rootVisualElement.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
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

			SceneManager.LoadScene("Main Menu", LoadSceneMode.Additive);
		}

		private void OnDestroy()
		{
			_inputController.UnsubscribeAction("Cancel", "UI", _inputCallback);
		}
	}
}