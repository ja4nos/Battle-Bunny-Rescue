using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BBR.GameLoop
{
	public static class PlayerHelper
	{
		private static readonly Dictionary<int, int> _playerIdToColorIdLookup = new();

		private static readonly Color[] _playerColors =
		{
			new(0.9f, 0.1f, 0.1f),
			new(0.1f, 0.4f, 0.9f),
			new(0.1f, 0.8f, 0.2f),
			new(0.9f, 0.8f, 0.1f),
			new(0.6f, 0.2f, 0.9f),
			new(0.1f, 0.8f, 0.8f),
			new(0.9f, 0.5f, 0.1f),
			new(0.9f, 0.4f, 0.7f)
		};

		public static Color GetPlayerColor(int playerId)
		{
			int playerColorId = _playerIdToColorIdLookup.GetValueOrDefault(playerId, playerId);
			return _playerColors[playerColorId];
		}

		public static Color GetNextPlayerColor(int playerId)
		{
			int currentIndex = _playerIdToColorIdLookup.GetValueOrDefault(playerId, 0);
			return GetNextPlayerColorFromIndex(currentIndex);
		}

		private static Color GetNextPlayerColorFromIndex(int currentIndex)
		{
			// Fallback to avoid stack overflow if somehow we have 8 players registered here...
			if(_playerIdToColorIdLookup.Count < _playerColors.Length)
			{
				currentIndex %= _playerColors.Length;

				foreach(int colorIndex in _playerIdToColorIdLookup.Values)
				{
					if(colorIndex.Equals(currentIndex))
					{
						currentIndex++;
						return GetNextPlayerColorFromIndex(currentIndex);
					}
				}
			}

			return _playerColors[currentIndex];
		}

		public static Color GetPreviousPlayerColor(int playerId)
		{
			int currentIndex = _playerIdToColorIdLookup.GetValueOrDefault(playerId, 0);
			return GetPreviousPlayerColorFromInex(currentIndex);
		}

		private static Color GetPreviousPlayerColorFromInex(int currentIndex)
		{
			// Fallback to avoid stack overflow if somehow we have 8 players registered here...
			if(_playerIdToColorIdLookup.Count < _playerColors.Length)
			{
				if(currentIndex < 0)
				{
					currentIndex += _playerColors.Length;
				}

				foreach(int colorIndex in _playerIdToColorIdLookup.Values)
				{
					if(colorIndex.Equals(currentIndex))
					{
						currentIndex--;
						return GetPreviousPlayerColorFromInex(currentIndex);
					}
				}
			}

			return _playerColors[currentIndex];
		}

		public static void RegisterPlayerColor(int playerId, Color playerColor)
		{
			int colorId = Math.Max(_playerColors.IndexOf(playerColor), 0);
			_playerIdToColorIdLookup[playerId] = colorId;
		}

		public static void ClearPlayerColors()
		{
			_playerIdToColorIdLookup.Clear();
		}

		public static void SetPlayerColor(GameObject playerInstance, int playerId)
		{
			playerId = Mathf.Clamp(playerId, 0, _playerColors.Length - 1);

			if(!_playerIdToColorIdLookup.TryGetValue(playerId, out int playerColorId))
			{
				Debug.LogError($"No color was registered for player with the id {playerId}. Using default instead!");
				playerColorId = playerId;
				_playerIdToColorIdLookup[playerId] = playerColorId;
			}

			const string basketsName = "Baskets";
			Renderer[] renderers = playerInstance.GetComponentsInChildren<Renderer>();
			Renderer basketsRenderer = renderers.FirstOrDefault(r => r.gameObject.name == basketsName);

			if(!basketsRenderer)
			{
				Debug.LogError($"No renderer with the name {basketsName} was found on {playerInstance.name}. Using first found renderer instead!");
				basketsRenderer = renderers.First();
			}

			basketsRenderer.material.color = _playerColors[playerColorId];
		}
	}
}