using BBR.Events;
using BBR.Events.Camera;
using Project.Input;
using System;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

namespace BBR.CameraController
{
	[Serializable]
	public class PlayerCamera : MonoBehaviour
	{
		[SerializeField] private GameObject _cameraPrefab;
		[SerializeField] private GameObject _cinemachineCameraPrefab;
		[SerializeField] private float _smoothSpeed = 5f;

		public RenderTexture RenderTexture { get; private set; }

		private Camera _camera;
		private int _playerIndex;
		private CinemachineImpulseSource _cinemachineImpulseSource;
		private Transform _smoothTarget;
		private Transform _target;

		internal void Setup(Transform followTransform, Vector2 imageScale, int playerIndex, InputController inputController)
		{
			int width = (int) (Screen.width * imageScale.x);
			int height = (int) (Screen.height * imageScale.y);
			RenderTexture = new RenderTexture(width, height, 24);
			_camera = Instantiate(_cameraPrefab, transform).GetComponent<Camera>();
			_camera.targetTexture = RenderTexture;

			_playerIndex = playerIndex;

			CinemachineCamera cinemachineCamera = Instantiate(_cinemachineCameraPrefab, transform).GetComponent<CinemachineCamera>();
			_smoothTarget = new GameObject("Camera Smooth Target").transform;
			_smoothTarget.parent = transform;
			_target = followTransform;
			cinemachineCamera.Follow = _smoothTarget;
			cinemachineCamera.OutputChannel = (OutputChannels) (1 << (playerIndex + 1));

			CinemachineBrain brain = _camera.GetComponent<CinemachineBrain>();
			brain.ChannelMask = (OutputChannels) (1 << (playerIndex + 1));

			PlayerCinemachineInputProvider inputProvider = cinemachineCamera.GetComponent<PlayerCinemachineInputProvider>();
			inputProvider.Init(playerIndex, inputController);

			CinemachineImpulseListener cinemachineImpulseListener = cinemachineCamera.GetComponent<CinemachineImpulseListener>();
			cinemachineImpulseListener.ChannelMask = playerIndex + 1;

			_cinemachineImpulseSource = cinemachineCamera.GetComponentInChildren<CinemachineImpulseSource>();
			_cinemachineImpulseSource.ImpulseDefinition.ImpulseChannel = playerIndex + 1;

			EventBus.Register<CameraShakeEvent>(OnCameraShake);
		}

		private void OnCameraShake(CameraShakeEvent shakeEvent)
		{
			if(shakeEvent.AffectedPlayers.Contains(_playerIndex))
			{
				_cinemachineImpulseSource.GenerateImpulseAtPositionWithVelocity(_cinemachineImpulseSource.transform.position, Vector3.one * shakeEvent.Force);
			}
		}

		private void LateUpdate()
		{
			_smoothTarget.position = new Vector3(_target.position.x, Mathf.Lerp(_smoothTarget.position.y, _target.position.y, Time.deltaTime * _smoothSpeed), _target.position.z);
			_smoothTarget.rotation = _target.rotation;
		}

		private void OnDestroy()
		{
			EventBus.Unregister<CameraShakeEvent>(OnCameraShake);

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
