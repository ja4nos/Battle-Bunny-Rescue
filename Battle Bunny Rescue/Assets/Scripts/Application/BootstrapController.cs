using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Application
{
	public class BootstrapController : MonoBehaviour
	{
		private void Awake()
		{
			SceneManager.LoadScene("Main Menu", LoadSceneMode.Additive);
			SceneManager.LoadScene("InputScene", LoadSceneMode.Additive);
		}
	}
}