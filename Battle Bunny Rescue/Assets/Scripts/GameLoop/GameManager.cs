using BBR.CameraController;
using BBR.GameLoop.Models;
using BBR.Movement;
using UnityEngine;
using Zenject;

namespace BBR.GameLoop
{
	public class GameManager : MonoBehaviour
	{
		[SerializeField] private CameraManager _cameraManager;
		[SerializeField] private GameObject _playerGameplayPrefab;
		[SerializeField] private Transform[] _spawnLocations;
		[SerializeField] private Vector3 _offset;

		[Inject] private DiContainer _diContainer;

		public void Init(PlayerInfo[] playerInfo)
		{
			Transform[] players = new Transform[playerInfo.Length];

			int i = 0;
			for(; i < playerInfo.Length; i++)
			{
				PlayerInfo info = playerInfo[i];
				GameObject player = _diContainer.InstantiatePrefab(_playerGameplayPrefab);
				players[i] = player.transform;

				BunnyMovementController playerMovement = player.GetComponent<BunnyMovementController>();
				playerMovement.Init(info.Id);

				BunnyPlayer bunnyPlayer = player.GetComponent<BunnyPlayer>();
				bunnyPlayer.Init(info.Id);

				Transform spawnLocation = _spawnLocations[i];

				player.transform.SetParent(transform);
				player.gameObject.SetActive(false);
				player.transform.SetPositionAndRotation(spawnLocation.position + _offset, spawnLocation.rotation);
				player.gameObject.SetActive(true);

				ChangeSpawnColor(spawnLocation, info.Color);
			}

			for(; i < _spawnLocations.Length; i++)
			{
				ChangeSpawnColor(_spawnLocations[i], Color.white);
			}

			_cameraManager.SetFor(players);
		}

		private static void ChangeSpawnColor(Transform spawnLocation, Color color)
		{
			Renderer spawnRenderer = spawnLocation.gameObject.GetComponent<Renderer>();
			spawnRenderer.material.color = color;
		}
	}
}