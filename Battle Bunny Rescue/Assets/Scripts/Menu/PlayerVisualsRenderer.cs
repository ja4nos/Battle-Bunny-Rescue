using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Project.Menu
{
	public class PlayerVisualsRenderer : IDisposable
	{
		private readonly GameObject _playerVisualsPrefab;
		private readonly Transform _parentTransform;
		private readonly int _playerId;

		private bool _initialized;
		private Image _rendererImage;
		private GameObject _playerVisualsInstance;
		private Camera _playerRendererCamera;
		private RenderTexture _playerRenderTexture;

		public PlayerVisualsRenderer(GameObject playerVisualsPrefab, Transform parentTransform, int playerId)
		{
			_playerVisualsPrefab = playerVisualsPrefab;
			_parentTransform = parentTransform;
			_playerId = playerId;
		}

		public void OnEnable(VisualElement root)
		{
			_rendererImage = root.Q<Image>(name: "player-render-image");
			_rendererImage.RegisterCallback<GeometryChangedEvent>(OnRenderImageGeometryChanged);
		}

		private void OnRenderImageGeometryChanged(GeometryChangedEvent evt)
		{
			bool shown = evt.newRect is { width: > 0, height: > 0 };
			SetShown(shown);
		}

		private void Initialize()
		{
			if(_initialized)
			{
				return;
			}

			Transform playerHolder = new GameObject($"Player {_playerId}").transform;
			playerHolder.SetParent(_parentTransform);
			playerHolder.Translate(Vector3.down * 100f);

			// Player Visuals
			_playerVisualsInstance = Object.Instantiate(_playerVisualsPrefab, playerHolder);
			_playerVisualsInstance.transform.localPosition = new Vector3(0, -2.5f, 5);

			int playerLayer = LayerMask.NameToLayer($"Player {_playerId}");

			foreach(Renderer renderer in _playerVisualsInstance.GetComponentsInChildren<Renderer>())
			{
				renderer.gameObject.layer = playerLayer;
			}

			// Render Texture
			CreateRenderTexture((int) _rendererImage.resolvedStyle.width, (int) _rendererImage.resolvedStyle.height);

			// Camera
			_playerRendererCamera = new GameObject("Render Camera", typeof(Camera), typeof(HDAdditionalCameraData), typeof(Volume)).GetComponent<Camera>();
			_playerRendererCamera.transform.SetParent(playerHolder, false);
			_playerRendererCamera.transform.rotation = Quaternion.Euler(16f, 0, 0);
			_playerRendererCamera.targetTexture = _playerRenderTexture;
			_playerRendererCamera.cullingMask = 1 << playerLayer;
			_playerRendererCamera.clearFlags = CameraClearFlags.SolidColor;
			_playerRendererCamera.gameObject.layer = playerLayer;

			HDAdditionalCameraData data = _playerRendererCamera.GetComponent<HDAdditionalCameraData>();
			data.exposureTarget = _playerVisualsInstance;
			data.backgroundColorHDR = new Color(1, 1, 1, 0);
			data.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
			data.volumeLayerMask = 1 << playerLayer;

			Volume volume = _playerRendererCamera.GetComponent<Volume>();
			volume.isGlobal = true;
			volume.priority = 100f;
			volume.profile = Resources.Load<VolumeProfile>("Player Selection Volume Profile");

			_initialized = true;
		}

		private void CreateRenderTexture(int width, int height)
		{
			_playerRenderTexture = new RenderTexture(width, height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt);
			_playerRenderTexture.Create();
			_rendererImage.image = _playerRenderTexture;
		}

		private void SetShown(bool shown)
		{
			Initialize();

			if(shown)
			{
				int newWidth = (int) _rendererImage.resolvedStyle.width;
				int newHeight = (int) _rendererImage.resolvedStyle.height;

				if(_playerRenderTexture.width != newWidth || _playerRenderTexture.height != newHeight)
				{
					_playerRenderTexture.Release();
					CreateRenderTexture(newWidth, newHeight);
				}

				UniTask.NextFrame().ContinueWith(() => _playerRendererCamera.enabled = true).Forget();
				SetReady(false);
			}
			else
			{
				DOTween.Kill(_playerVisualsInstance);
				_playerRendererCamera.enabled = false;
			}
		}

		public void SetReady(bool ready)
		{
			if(!_initialized)
			{
				return;
			}

			DOTween.Kill(_playerVisualsInstance);

			if(ready)
			{
				const float spinDuration = 2.5f;
				float angleDifference = Quaternion.Angle(_playerVisualsInstance.transform.rotation, Quaternion.Euler(new Vector3(0, 180, 0)));

				if(_playerVisualsInstance.transform.rotation.eulerAngles.y > 180)
				{
					angleDifference = 360 - angleDifference;
				}

				float duration = angleDifference / 360f * spinDuration;

				_playerVisualsInstance.transform.DOLocalRotate(new Vector3(0, angleDifference, 0), spinDuration + duration, RotateMode.LocalAxisAdd).SetEase(Ease.OutElastic).SetId(_playerVisualsInstance);
			}
			else
			{
				_playerVisualsInstance.transform.DOLocalRotate(new Vector3(0, 360, 0), 8f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental).SetId(_playerVisualsInstance);
			}
		}

		public void Dispose()
		{
			if(_playerVisualsInstance)
			{
				DOTween.Kill(_playerVisualsInstance);
				Object.Destroy(_playerVisualsInstance);
			}

			if(_playerRenderTexture)
			{
				_playerRenderTexture.Release();
				Object.Destroy(_playerRenderTexture);
			}

			_playerVisualsInstance = null;
			_playerRendererCamera = null;
			_playerRenderTexture = null;
			_initialized = false;
		}
	}
}