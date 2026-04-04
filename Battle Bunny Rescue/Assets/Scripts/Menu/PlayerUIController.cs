using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Menu
{
	public class PlayerUIController : MonoBehaviour
	{
		[SerializeField] private UIDocument _uiDocument;
		[SerializeField] private VisualTreeAsset _playerUIAsset;

		private void Awake()
		{
			if(_uiDocument == null)
			{
				Debug.LogError($"{nameof(UIDocument)} has been assigned to {nameof(PlayerUIController)}!", this);
			}

			if(_playerUIAsset == null)
			{
				Debug.LogError($"{nameof(VisualTreeAsset)} has been assigned to {nameof(PlayerUIController)}!", this);
			}
		}

		public void Init(int playerCount)
		{
			for(int i = 0; i < playerCount; i++)
			{
				VisualElement playerUIInstance = _playerUIAsset.Instantiate();
				//playerUIInstance.style.
			}
		}
	}
}