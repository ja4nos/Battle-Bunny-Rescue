using BBR.Events;
using BBR.Events.Camera;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace BBR.CameraController
{
	internal class EventCamera : MonoBehaviour
	{
		[SerializeField] private float _rotationSpeed = 30;
		[SerializeField] private GameObject _cameraPrefab;
		[SerializeField] private GameObject _cinemachineCameraPrefab;
		[SerializeField] private Vector2Int _size = new(512, 512);
		[SerializeField] private AnimationCurve _showCurve;
		[SerializeField] private AnimationCurve _hideCurve;

		public RenderTexture RenderTexture { get; private set; }

		private Camera _camera;
		private CinemachineCamera _cinemachineCamera;
		private RectTransform _canvasTransform;
		private Transform _defaultFollowTransform;

		private Tween _tween;
		private float _showTime;

		private void Awake()
		{
			EventBus.Register<CameraShowEvent>(OnCameraShow);
		}

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
			_canvasTransform.gameObject.SetActive(false);
			enabled = false;

			_cinemachineCamera = Instantiate(_cinemachineCameraPrefab, transform).GetComponent<CinemachineCamera>();

		}

		private void OnCameraShow(CameraShowEvent args)
		{
			if(args.Transform)
			{
				Show(args.Transform, args.Time);
			}
			else
			{
				Show(args.Position, args.Time);
			}
		}

		private void Show(Vector3 position, float time)
		{
			_defaultFollowTransform.position = position;
			Show(_defaultFollowTransform, time);
		}

		private void Show(Transform focusTransform, float time)
		{
			_cinemachineCamera.Follow = focusTransform;
			_cinemachineCamera.GetComponent<CinemachinePositionComposer>().ForceCameraPosition(focusTransform.position, focusTransform.rotation);
			_showTime = time;
			ShowDelay().Forget();
		}

		// Wait for camera to move to the correct spot
		private async UniTask ShowDelay()
		{
			await UniTask.DelayFrame(4);
			_canvasTransform.localScale = Vector3.zero;
			_canvasTransform.gameObject.SetActive(true);
			enabled = true;

			KillTween();
			_tween = _canvasTransform.DOScale(1, 0.5f)
				.SetEase(_showCurve);
		}

		private void Hide()
		{
			KillTween();
			_tween = _canvasTransform.DOScale(0, 0.25f)
				.SetEase(_hideCurve)
				.OnComplete(() =>
				{
					_canvasTransform.gameObject.SetActive(false);
					enabled = false;
				});
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

		private void KillTween()
		{
			if(_tween != null && _tween.IsActive())
			{
				_tween.Kill();
			}
		}

		private void OnDestroy()
		{
			Hide();

			EventBus.Unregister<CameraShowEvent>(OnCameraShow);

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
