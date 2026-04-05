#if UNITY_EDITOR
using JetBrains.Annotations;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Utilities
{
	public static class SceneUtilities
	{
		private const string _sceneDropdownPath = "Scene Utilities/Scene Selector";
		private const string _bootstrapPath = "Scene Utilities/Bootstrap";

		private const string _iconPathBootstrapBtn = "Editor/ic_bootstrap";
		private const string _tooltipBootstrapBtn = "Start the application flow";
		private const string _tooltipSceneBtn = "Select and open a scene in build settings";

		private static SceneAsset _selectedScene;
		private static bool _isEnteringPlayModeManually;

		private static string _activeSceneName;

		static SceneUtilities()
		{
			SceneManager.activeSceneChanged -= OnActiveSceneChanged;
			SceneManager.activeSceneChanged += OnActiveSceneChanged;
			EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
			EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
		}

		[UsedImplicitly]
		[MainToolbarElement(_sceneDropdownPath, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = 1001)]
		private static MainToolbarElement SceneToolbarGUI()
		{
			UpdateActiveSceneName();

			Texture2D icon = EditorGUIUtility.IconContent("UnityLogo").image as Texture2D;
			MainToolbarContent content = new(_activeSceneName, icon, _tooltipSceneBtn);
			return new MainToolbarDropdown(content, ShowSceneDropdownMenu);
		}

		private static void UpdateActiveSceneName()
		{
			Scene s = SceneManager.GetActiveScene();
			_activeSceneName = string.IsNullOrEmpty(s.name) ? "Untitled" : s.name;
		}

		private static void OnActiveSceneChanged(Scene _, Scene newScene)
		{
			_activeSceneName = string.IsNullOrEmpty(newScene.name) ? "Untitled" : newScene.name;
			MainToolbar.Refresh(_sceneDropdownPath);
		}

		private static void ShowSceneDropdownMenu(Rect dropDownRect)
		{
			GenericMenu menu = new();
			AddBuilderScenes(menu);
			menu.DropDown(dropDownRect);
		}

		private static void AddBuilderScenes(GenericMenu menu)
		{
			foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
			{
				string path = scene.path;
				string name = GetNameFromPath(path);
				menu.AddItem(new GUIContent(name), false, () => { SceneHelper.OpenScene(path); });
			}
		}

		private static string GetNameFromPath(string scenePath)
		{
			int startIndex = scenePath.LastIndexOf("/", StringComparison.Ordinal) + 1;
			string restString = scenePath[startIndex..];
			return restString.Remove(restString.IndexOf(".", StringComparison.Ordinal));
		}

		[UsedImplicitly]
		[MainToolbarElement(_bootstrapPath, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = -1)]
		private static MainToolbarElement BootstrapToolbarGUI()
		{
			Texture2D icon = Resources.Load<Texture2D>(_iconPathBootstrapBtn);
			MainToolbarContent content = new(icon, _tooltipBootstrapBtn);
			return new MainToolbarButton(content, StartBootstrapScene);
		}

		[MenuItem("Scene Utilities/Start Bootstrap %h")]
		private static void StartBootstrapScene()
		{
			SceneHelper.StartScene(GetBootScenePath());
		}

		private static string GetBootScenePath()
		{
			if(EditorBuildSettings.scenes.Length <= 0)
			{
				throw new Exception("No scenes are added to this project's build settings");
			}

			string scenePath = string.Empty;

			foreach(EditorBuildSettingsScene editorBuildSettingsScene in EditorBuildSettings.scenes)
			{
				if(!editorBuildSettingsScene.enabled)
				{
					continue;
				}

				scenePath = editorBuildSettingsScene.path;
				break;
			}

			if(string.IsNullOrEmpty(scenePath))
			{
				throw new Exception("No scenes were enabled in the build settings");
			}

			return scenePath;
		}
	}
}
#endif
