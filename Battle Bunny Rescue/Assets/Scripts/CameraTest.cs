using BBR.CameraController;
using UnityEngine;

namespace BBR
{
	public class CameraTest : MonoBehaviour
	{
		[SerializeField] private CameraManager _cameraManager;
		[SerializeField] private Transform[] _players;

		private void Start()
		{
			_cameraManager.SetFor(_players);
		}
	}
}
