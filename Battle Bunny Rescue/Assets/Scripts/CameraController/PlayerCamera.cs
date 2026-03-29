using System;
using UnityEngine;

namespace BBR.CameraController
{
	[Serializable]
	public class PlayerCamera : MonoBehaviour
	{
		[SerializeField] private float _followSpeed = 1;
		[SerializeField] private Camera _cameraPrefab;
		[SerializeField] private Vector3 _followOffset;
		[SerializeField] private Vector3 _followRotation;

		public RenderTexture RenderTexture { get; private set; }
		public Camera Camera { get; private set; }

		private Transform _followTransform;

		public void Setup(Transform followTransform, Vector2 imageScale)
		{
			_followTransform = followTransform;

			int width = (int) (Screen.width * imageScale.x);
			int height = (int) (Screen.height * imageScale.y);
			RenderTexture = new RenderTexture(width, height, 24);
			Camera = Instantiate(_cameraPrefab, transform);
			Camera.targetTexture = RenderTexture;

			Camera.transform.position = followTransform.position + _followOffset;
			Camera.transform.rotation = Quaternion.Euler(_followRotation);
		}

		private void LateUpdate()
		{
			Camera.transform.position = Vector3.Lerp(Camera.transform.position, _followTransform.position + _followOffset, Time.deltaTime * _followSpeed);
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