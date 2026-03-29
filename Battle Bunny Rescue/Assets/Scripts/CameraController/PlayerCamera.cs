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
		public Camera Camera { get; private set; }

		public void Setup(Transform followTransform, Vector2 imageScale, int playerIndex)
		{
			int width = (int) (Screen.width * imageScale.x);
			int height = (int) (Screen.height * imageScale.y);
			RenderTexture = new RenderTexture(width, height, 24);
			Camera = Instantiate(_cameraPrefab, transform).GetComponent<Camera>();
			Camera.targetTexture = RenderTexture;

			CinemachineCamera cinemachineCamera = Instantiate(_cinemachineCameraPrefab, transform).GetComponent<CinemachineCamera>();
			cinemachineCamera.Follow = followTransform;
			cinemachineCamera.OutputChannel = (OutputChannels) (1 << playerIndex + 1);

			CinemachineBrain brain = Camera.GetComponent<CinemachineBrain>();
			brain.ChannelMask = (OutputChannels) (1 << playerIndex + 1);
		}

		private void OnDestroy()
		{
			if(RenderTexture)
			{
				RenderTexture.Release();
				Destroy(RenderTexture);
			}

			if(Camera)
			{
				Destroy(Camera);
			}
		}
	}
}
