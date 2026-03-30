using BBR.CameraController;
using UnityEngine;

namespace BBR.GameLoop
{
	public class GameManager : MonoBehaviour
	{
		[SerializeField] private CameraManager _cameraManager;
		[SerializeField] private Transform[] _spawnLocations;
		[SerializeField] private Vector3 _offset;

		public void Init(Transform[] players)
		{
			_cameraManager.SetFor(players);

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