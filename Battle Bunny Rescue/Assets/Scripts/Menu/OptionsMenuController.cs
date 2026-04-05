using BBR.AudioPlayer;
using UnityEngine.UIElements;

namespace Project.Menu
{
	public class OptionsMenuController
	{
		private VisualElement _root;
		private VisualElement _panel;
		private Slider _slider;

		private bool _shown;

		public void OnEnable(VisualElement root)
		{
			_root = root;
			_panel = root.Q<VisualElement>(name: "panel");
			_slider = _panel.Q<Slider>();
			_slider.RegisterValueChangedCallback(evt => { AudioPlayer.SetVolume("MasterVolume", evt.newValue, true); });

			root.RegisterCallback<NavigationCancelEvent>(_ => { SetShown(false); });
			root.RegisterCallback<ClickEvent>(evt =>
			{
				if(evt.target == root)
				{
					SetShown(false);
				}
			});

			SetShown(_shown);
		}

		public void SetShown(bool shown)
		{
			_shown = shown;
			_root.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;

			if(shown)
			{
				_slider.Focus();
				_slider.value = AudioPlayer.GetVolume("MasterVolume");
			}
		}
	}
}