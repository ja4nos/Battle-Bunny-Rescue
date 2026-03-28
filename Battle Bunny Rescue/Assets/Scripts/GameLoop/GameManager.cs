using BBR.CameraController;
using System.Linq;
using UnityEngine;

namespace BBR
{
	public class GameManager : MonoBehaviour
	{
		[SerializeField] private TopDownCarController[] _players;
		[SerializeField] private CameraManager _cameraManager;

		private void Start()
		{
			_cameraManager.SetFor(_players.Select(x => x.transform).ToArray());
		}
	}
}
