using BBR.AudioPlayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Project.Menu
{
	public class MainMenuController : MonoBehaviour
	{
		[SerializeField] private UIDocument _menuUIDocument;
		[SerializeField] private AudioHolder _clickSfx;

		private OptionsMenuController _optionsMenuController;

		private void Awake()
		{
			if(_menuUIDocument == null)
			{
				Debug.LogError($"No main menu UI document has been assigned to {nameof(MainMenuController)}!", this);
				Destroy(this);
			}

			_optionsMenuController = new OptionsMenuController();
		}

		private void OnEnable()
		{
			_optionsMenuController.OnEnable(_menuUIDocument.rootVisualElement.Q<VisualElement>(name: "options-menu"));
			Button playButton = _menuUIDocument.rootVisualElement.Q<Button>(name: "play-button");
			Button optionsButton = _menuUIDocument.rootVisualElement.Q<Button>(name: "options-button");
			Button exitButton = _menuUIDocument.rootVisualElement.Q<Button>(name: "exit-button");

			playButton.clicked += OnPlayClicked;
			optionsButton.clicked += OnOptionsClicked;
			exitButton.clicked += OnExitClicked;

			playButton.Focus();

			_menuUIDocument.rootVisualElement.RegisterCallback<FocusEvent>(_ => { _clickSfx.Play(); }, TrickleDown.TrickleDown);
			_menuUIDocument.rootVisualElement.RegisterCallback<NavigationSubmitEvent>(_ => { _clickSfx.Play(); }, TrickleDown.TrickleDown);
		}

		private static void OnPlayClicked()
		{
			SceneManager.UnloadSceneAsync("Main Menu");
			SceneManager.LoadSceneAsync("Player Selection Menu", LoadSceneMode.Additive);
		}

		private void OnOptionsClicked()
		{
			_optionsMenuController.SetShown(true);
		}

		private static void OnExitClicked() => Application.Quit();
	}
}