using UnityEngine.UIElements;

namespace Project.Menu
{
	public class PlayerUIController
	{
		private int _playerId;

		public PlayerUIController(int playerId)
		{
			_playerId = playerId;
		}

		public void OnEnable(VisualElement root)
		{
		}
	}
}