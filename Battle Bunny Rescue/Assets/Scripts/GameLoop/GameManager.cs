using BBR.CameraController;
using BBR.GameLoop.Models;
using BBR.Movement;
using Project.Input;
using System;
using System.Linq;
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
		[Inject] private InputController _inputController;

		public void Init(PlayerInfo[] playerInfo)
		{
			Transform[] players = new Transform[playerInfo.Length];

			int i = 0;
			for(; i < playerInfo.Length; i++)
			{
				PlayerInfo info = playerInfo[i];
				GameObject player = _diContainer.InstantiatePrefab(_playerGameplayPrefab);
				players[i] = player.transform;

				BunnyMovementPlayer playerMovement = player.GetComponent<BunnyMovementPlayer>();
				playerMovement.Init(info.Id);

				Transform spawnLocation = _spawnLocations[i];

				BunnyPlayer bunnyPlayer = player.GetComponent<BunnyPlayer>();
				bunnyPlayer.Init(info.Id, spawnLocation);

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

			_cameraManager.SetFor(players, _inputController);
		}

		private static void ChangeSpawnColor(Transform spawnLocation, Color color)
		{
			Renderer[] renderers = spawnLocation.GetComponentsInChildren<Renderer>();

			Renderer flags1 = renderers.First(r => string.Equals(r.name, "Plane.002", StringComparison.Ordinal)).GetComponent<Renderer>();
			Renderer flags2 = renderers.First(r => string.Equals(r.name, "Plane.003", StringComparison.Ordinal)).GetComponent<Renderer>();
			Renderer cube = renderers.First(r => string.Equals(r.name, "Spawn Cube", StringComparison.Ordinal)).GetComponent<Renderer>();

			flags1.material.color = color;
			flags2.material.color = color;
			cube.material.color = color;
		}
	}
}