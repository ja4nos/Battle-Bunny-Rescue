using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Menu
{
	public class PlayerUIManager : MonoBehaviour
	{
		private const int _rowElementCount = 2;

		[SerializeField] private UIDocument _uiDocument;
		[SerializeField] private VisualTreeAsset _playerUIAsset;

		private readonly Dictionary<int, PlayerUIController> _playerUIControllers = new();

		private void Awake()
		{
			if(_uiDocument == null)
			{
				Debug.LogError($"{nameof(UIDocument)} has not been assigned to {nameof(PlayerUIController)}!", this);
			}

			if(_playerUIAsset == null)
			{
				Debug.LogError($"{nameof(VisualTreeAsset)} has not been assigned to {nameof(PlayerUIController)}!", this);
			}

			enabled = false;
		}

		public void Init(int playerCount)
		{
			VisualElement row = null;

			for(int i = 0; i < playerCount; i++)
			{
				if(i % _rowElementCount == 0)
				{
					row = new VisualElement();
					row.AddToClassList("row");
					_uiDocument.rootVisualElement.Add(row);
				}

				VisualElement playerUIInstance = _playerUIAsset.Instantiate();
				playerUIInstance.name = $"player-{i}";
				row!.Add(playerUIInstance);

				_playerUIControllers.Add(i, new PlayerUIController(i));
			}

			if(playerCount > _rowElementCount && row is { childCount: 1 })
			{
				VisualElement freeSpace = new() { style = { flexGrow = 1, width = new StyleLength(new Length(100, LengthUnit.Percent)) } };
				row.Add(freeSpace);
			}

			enabled = true;
		}

		private void OnEnable()
		{
			for(int i = 0; i < _playerUIControllers.Count; i++)
			{
				_playerUIControllers[i].OnEnable(_uiDocument.rootVisualElement.Q(name: $"player-{i}")[0]);
			}
		}
	}
}