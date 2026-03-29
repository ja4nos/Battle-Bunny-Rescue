using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace BBR.CameraController
{
	public class EventCamera : MonoBehaviour
	{
		[SerializeField] private float _rotationSpeed = 30;
		[SerializeField] private GameObject _cameraPrefab;
		[SerializeField] private GameObject _cinemachineCameraPrefab;
		[SerializeField] private Vector2Int _size = new(512, 512);

		public RenderTexture RenderTexture { get; private set; }

		private Camera _camera;
		private CinemachineCamera _cinemachineCamera;
		private RectTransform _canvasTransform;
		private Transform _defaultFollowTransform;

		private float _showTime;

		internal void Setup(RectTransform canvasTransform, Vector2 positionOnScreen)
		{
			_defaultFollowTransform = new GameObject("EventCameraRotationPoint").transform;
			_defaultFollowTransform.SetParent(transform);
			_canvasTransform = canvasTransform;
			RenderTexture = new RenderTexture(_size.x, _size.y, 24);
			_camera = Instantiate(_cameraPrefab, transform).GetComponent<Camera>();
			_camera.targetTexture = RenderTexture;
			_canvasTransform.GetComponentInChildren<RawImage>().texture = RenderTexture;
			_canvasTransform.sizeDelta = new Vector2(_size.x, _size.y);
			_canvasTransform.anchoredPosition = positionOnScreen;

			_cinemachineCamera = Instantiate(_cinemachineCameraPrefab, transform).GetComponent<CinemachineCamera>();
		}

		public void Show(Vector3 position, float time)
		{
			_defaultFollowTransform.position = position;
			Show(_defaultFollowTransform, time);
		}

		public void Show(Transform focusTransform, float time)
		{
			_cinemachineCamera.Follow = focusTransform;
			_showTime = time;
			_canvasTransform.gameObject.SetActive(true);
			enabled = true;
		}

		public void Hide()
		{
			_canvasTransform.gameObject.SetActive(false);
			enabled = false;
		}

		private void Update()
		{
			_defaultFollowTransform.RotateAround(_defaultFollowTransform.position, Vector3.up, _rotationSpeed * Time.deltaTime);

			_showTime -= Time.deltaTime;
			if(_showTime <= 0)
			{
				Hide();
			}
		}

		private void OnDestroy()
		{
			if(RenderTexture)
			{
				Destroy(RenderTexture);
			}

			if(_camera)
			{
				Destroy(_camera.gameObject);
			}
		}
	}
}
