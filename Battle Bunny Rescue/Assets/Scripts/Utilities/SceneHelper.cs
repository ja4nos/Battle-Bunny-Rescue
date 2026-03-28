using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Project.Utilities
{
	/// <summary>
	/// Used for opening and starting scenes safely.
	/// </summary>
	[InitializeOnLoad]
	internal static class SceneHelper
	{
		private const string _bootstrapLoadRequested = "BootstrapLoadRequested";
		private static string _sceneToOpen;
		private static bool _shouldStartPlaying;

		static SceneHelper()
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		public static void OpenScene(string scene)
		{
			if(EditorApplication.isPlaying)
			{
				EditorApplication.isPlaying = false;
			}

			_sceneToOpen = scene;
			_shouldStartPlaying = false;
			EditorApplication.update += OnUpdate;
		}

		public static void StartScene(string scene)
		{
			if(EditorApplication.isPlaying)
			{
				SaveBootstrapPreference(true);
				EditorApplication.isPlaying = false;
			}

			EditorPrefs.SetString("oldScene", SceneManager.GetActiveScene().path);
			_sceneToOpen = scene;
			_shouldStartPlaying = true;
			EditorApplication.update += OnUpdate;
		}

		private static void OnUpdate()
		{
			if(_sceneToOpen == null || EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			EditorApplication.update -= OnUpdate;

			if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				EditorSceneManager.OpenScene(_sceneToOpen);
				EditorApplication.isPlaying = _shouldStartPlaying;
			}

			_sceneToOpen = null;
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
		{
			if(stateChange == PlayModeStateChange.EnteredEditMode)
			{
				string oldScene = EditorPrefs.GetString("oldScene");

				if(!string.IsNullOrEmpty(oldScene))
				{
					EditorSceneManager.OpenScene(oldScene);
				}

				EditorPrefs.SetString("oldScene", string.Empty);
			}
		}

		private static void SaveBootstrapPreference(bool startBootstrap)
		{
			SessionState.SetBool(_bootstrapLoadRequested, startBootstrap);
		}

		public static bool LoadBootstrapPreference()
		{
			if(EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isCompiling)
			{
				return false;
			}

			if(SessionState.GetBool(_bootstrapLoadRequested, false))
			{
				SaveBootstrapPreference(false);
				return true;
			}

			return false;
		}
	}
}