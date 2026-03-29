using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Editor
{
	public class UssReloader : AssetPostprocessor
	{
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			bool ussChanged = false;

			foreach(string asset in importedAssets)
			{
				if(asset.EndsWith(".uss"))
				{
					ussChanged = true;
					break;
				}
			}

			if(ussChanged)
			{
				EditorApplication.delayCall -= Callback;
				EditorApplication.delayCall += Callback;
			}
		}

		private static void Callback()
		{
			EditorApplication.delayCall -= Callback;

			bool originalLogState = Debug.unityLogger.logEnabled;
			UIDocument[] uiDocuments = Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include);

			foreach(UIDocument doc in uiDocuments)
			{
				if(doc.panelSettings != null && doc.panelSettings.themeStyleSheet != null)
				{
					ThemeStyleSheet currentTheme = doc.panelSettings.themeStyleSheet;

					try
					{
						// Temporarily disable logging to silence the "No Theme" warning
						Debug.unityLogger.logEnabled = false;

						// Remove the theme to force the panel to redraw
						doc.panelSettings.themeStyleSheet = null;
					}
					finally
					{
						// Restore logging
						Debug.unityLogger.logEnabled = originalLogState;
					}

					// Re-assign the proper theme, as if it never happened
					doc.panelSettings.themeStyleSheet = currentTheme;
				}
			}
		}
	}
}