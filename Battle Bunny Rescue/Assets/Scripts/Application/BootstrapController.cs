using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Application
{
	public class BootstrapController : MonoBehaviour
	{
		private void Awake()
		{
			SceneManager.LoadSceneAsync("Main Menu", LoadSceneMode.Additive);
			SceneManager.LoadSceneAsync("InputScene", LoadSceneMode.Additive);
		}
	}
}