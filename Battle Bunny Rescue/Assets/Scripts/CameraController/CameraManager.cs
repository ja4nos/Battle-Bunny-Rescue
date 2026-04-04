using Project.Input;
using UnityEngine;
using UnityEngine.UI;

namespace BBR.CameraController
{
	public class CameraManager : MonoBehaviour
	{
		[SerializeField] private GameObject _playerCameraPrefab;
		[SerializeField] private GameObject _outputTemplate;
		[SerializeField] private GameObject _eventCameraPrefab;
		[SerializeField] private GameObject _eventOutputTemplate;
		[SerializeField] private Transform _outputTransform;

		private RectTransform[] _rawImages;
		private PlayerCamera[] _playerCameras;
		private EventCamera _eventCamera;

		private static Vector2 GetScaleFor(int playerCount)
		{
			float x = playerCount > 1 ? 0.5f : 1f;
			float y = playerCount > 2 ? 0.5f : 1f;
			return new Vector2(x, y);
		}

		private void Start()
		{
			RectTransform rectTransform = Instantiate(_eventOutputTemplate, _outputTransform).transform as RectTransform;
			_eventCamera = Instantiate(_eventCameraPrefab, transform).GetComponent<EventCamera>();
			_eventCamera.Setup(rectTransform, Vector2.zero);
		}

		public void SetFor(Transform[] transformsToFollow, InputController inputController)
		{
			if(_playerCameras != null)
			{
				foreach(PlayerCamera playerCamera in _playerCameras)
				{
					Destroy(playerCamera.gameObject);
				}
			}

			if(_rawImages != null)
			{
				foreach(RectTransform rawImage in _rawImages)
				{
					Destroy(rawImage.gameObject);
				}
			}

			_playerCameras = new PlayerCamera[transformsToFollow.Length];
			_rawImages = new RectTransform[transformsToFollow.Length];

			for(int i = 0; i < transformsToFollow.Length; i++)
			{
				Transform t = transformsToFollow[i];
				_playerCameras[i] = Instantiate(_playerCameraPrefab, t.position, Quaternion.identity, transform).GetComponent<PlayerCamera>();
				_playerCameras[i].Setup(t, GetScaleFor(transformsToFollow.Length), i, inputController);
			}

			Vector2 scale = GetScaleFor(transformsToFollow.Length);

			for(int i = 0; i < transformsToFollow.Length; i++)
			{
				float minX = i % 2 * scale.x;
				float maxY = 1 - (int) (i / 2f) * scale.y;
				float maxX = minX + scale.x;
				float minY = maxY - scale.y;

				_rawImages[i] = Instantiate(_outputTemplate, _outputTransform).transform as RectTransform;
				_rawImages[i].anchorMin = new Vector2(minX, minY);
				_rawImages[i].anchorMax = new Vector2(maxX, maxY);
				_rawImages[i].sizeDelta = scale;
				_rawImages[i].anchoredPosition = Vector2.zero;
				_rawImages[i].GetComponent<RawImage>().texture = _playerCameras[i].RenderTexture;
			}
		}
	}
}