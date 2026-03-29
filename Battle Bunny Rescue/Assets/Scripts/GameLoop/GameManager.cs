using BBR.CameraController;
using UnityEngine;

namespace BBR
{
	public class GameManager : MonoBehaviour
	{
		[SerializeField] private CameraManager _cameraManager;

		public void Init(Transform[] players)
		{
			_cameraManager.SetFor(players);

			foreach(Transform player in players)
			{
				player.SetParent(transform);
			}
		}
	}
}