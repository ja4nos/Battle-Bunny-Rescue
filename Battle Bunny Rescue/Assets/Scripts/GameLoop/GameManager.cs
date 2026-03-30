using BBR.CameraController;
using Project.Input;
using UnityEngine;
using Zenject;

namespace BBR.GameLoop
{
	public class GameManager : MonoBehaviour
	{
		[SerializeField] private CameraManager _cameraManager;
		[SerializeField] private Transform[] _spawnLocations;
		[SerializeField] private Vector3 _offset;

		[Inject] private InputController _inputController;

		public void Init(Transform[] players)
		{
			_cameraManager.SetFor(players, _inputController);

			for(int i = 0; i < players.Length; i++)
			{
				players[i].SetParent(transform);
				players[i].gameObject.SetActive(false);
				players[i].SetPositionAndRotation(_spawnLocations[i].position + _offset, _spawnLocations[i].rotation);
				players[i].gameObject.SetActive(true);
			}
		}
	}
}