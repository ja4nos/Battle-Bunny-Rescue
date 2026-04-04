using Project.Input;
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace BBR.CameraController
{
	[Serializable]
	public class PlayerCamera : MonoBehaviour
	{
		[SerializeField] private GameObject _cameraPrefab;
		[SerializeField] private GameObject _cinemachineCameraPrefab;
		public RenderTexture RenderTexture { get; private set; }

		private Camera _camera;

		internal void Setup(Transform followTransform, Vector2 imageScale, int playerIndex, InputController inputController)
		{
			int width = (int) (Screen.width * imageScale.x);
			int height = (int) (Screen.height * imageScale.y);
			RenderTexture = new RenderTexture(width, height, 24);
			_camera = Instantiate(_cameraPrefab, transform).GetComponent<Camera>();
			_camera.targetTexture = RenderTexture;

			CinemachineCamera cinemachineCamera = Instantiate(_cinemachineCameraPrefab, transform).GetComponent<CinemachineCamera>();
			cinemachineCamera.Follow = followTransform;
			cinemachineCamera.OutputChannel = (OutputChannels) (1 << (playerIndex + 1));

			CinemachineBrain brain = _camera.GetComponent<CinemachineBrain>();
			brain.ChannelMask = (OutputChannels) (1 << (playerIndex + 1));

			PlayerCinemachineInputProvider inputProvider = cinemachineCamera.GetComponent<PlayerCinemachineInputProvider>();
			inputProvider.Init(playerIndex, inputController);
		}

		private void OnDestroy()
		{
			if(RenderTexture)
			{
				RenderTexture.Release();
				Destroy(RenderTexture);
			}

			if(_camera)
			{
				Destroy(_camera);
			}
		}
	}
}