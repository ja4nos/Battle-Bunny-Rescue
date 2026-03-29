using BBR.CameraController;
using UnityEngine;

namespace BBR
{
	public class GameManager : MonoBehaviour
	{
		[SerializeField] private Transform[] _players;
		[SerializeField] private CameraManager _cameraManager;

		private void Start()
		{
			_cameraManager.SetFor(_players);
		}
	}
}
