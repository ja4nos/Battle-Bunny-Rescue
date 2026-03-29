using UnityEngine;
using UnityEngine.UI;

namespace BBR.CameraController
{
	public class CameraManager : MonoBehaviour
	{
		[SerializeField] private GameObject _playerCameraPrefab;
		[SerializeField] private GameObject _outputTemplate;
		[SerializeField] private Transform _outputTransform;

		private RawImage[] _rawImages;
		private PlayerCamera[] _playerCameras;

		private static Vector2 GetScaleFor(int playerCount)
		{
			float x = playerCount > 1 ? 0.5f : 1f;
			float y = playerCount > 2 ? 0.5f : 1f;
			return new Vector2(x, y);
		}

		public void SetFor(Transform[] transformsToFollow)
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
				foreach(RawImage rawImage in _rawImages)
				{
					Destroy(rawImage.gameObject);
				}
			}

			_playerCameras = new PlayerCamera[transformsToFollow.Length];
			_rawImages = new RawImage[transformsToFollow.Length];

			for(int i = 0; i < transformsToFollow.Length; i++)
			{
				Transform t = transformsToFollow[i];
				_playerCameras[i] = Instantiate(_playerCameraPrefab, t.position, Quaternion.identity, transform).GetComponent<PlayerCamera>();
				_playerCameras[i].Setup(t, GetScaleFor(transformsToFollow.Length), i);
			}

			Vector2 scale = GetScaleFor(transformsToFollow.Length);

			for(int i = 0; i < transformsToFollow.Length; i++)
			{
				float minX = i % 2 * scale.x;
				float maxY = 1 - (int) (i / 2f) * scale.y;
				float maxX = minX + scale.x;
				float minY = maxY - scale.y;

				_rawImages[i] = Instantiate(_outputTemplate, _outputTransform).GetComponent<RawImage>();
				_rawImages[i].rectTransform.anchorMin = new Vector2(minX, minY);
				_rawImages[i].rectTransform.anchorMax = new Vector2(maxX, maxY);
				_rawImages[i].rectTransform.sizeDelta = scale;
				_rawImages[i].rectTransform.anchoredPosition = Vector2.zero;
				_rawImages[i].texture = _playerCameras[i].RenderTexture;
			}
		}
	}
}