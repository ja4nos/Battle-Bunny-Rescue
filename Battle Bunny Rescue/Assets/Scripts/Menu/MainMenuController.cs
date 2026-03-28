using Project.Utilities;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace BBR.Menu
{
	public class MainMenuController : MonoBehaviour
	{
		[SerializeField] private UIDocument _menuUIDocument;
		[SerializeField] private SceneGroup _gameSceneGroup;

		private void Awake()
		{
			if(_menuUIDocument == null)
			{
				Debug.LogError($"No main menu UI document has been assigned to {nameof(MainMenuController)}!", this);
				return;
			}

			Button playButton = _menuUIDocument.rootVisualElement.Q<Button>(name: "play-button");
			Button optionsButton = _menuUIDocument.rootVisualElement.Q<Button>(name: "options-button");
			Button exitButton = _menuUIDocument.rootVisualElement.Q<Button>(name: "exit-button");

			playButton.clicked += OnPlayClicked;
			optionsButton.clicked += OnOptionsClicked;
			exitButton.clicked += OnExitClicked;
		}

		private void OnPlayClicked()
		{
			SceneManager.UnloadSceneAsync("Main Menu");

			foreach(string sceneName in _gameSceneGroup.Scenes)
			{
				SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			}
		}

		private static void OnOptionsClicked()
		{
			throw new NotImplementedException();
		}

		private static void OnExitClicked() => Application.Quit();
	}
}