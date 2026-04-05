using BBR.Events;
using BBR.Events.Camera;
using Pool.Pool;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BBR.GameLoop
{
	public class BunnyPlayer : MonoBehaviour
	{
		public bool IsStunned => _stunTimeRemainingSeconds > 0f;
		public bool IsFull => _availableSpots.Count == 0;

		[SerializeField] private float _stunTimeSeconds = 3f;
		[SerializeField] private Transform[] _smallBunniesLocations;
		[SerializeField] private ParticlePool _deliveryParticles;

		private Transform _playerBase;
		private int _playerId;
		private int _capturedBunniesCount;
		private int _savedBunniesCount;
		private float _stunTimeRemainingSeconds;
		private CapturedBunniesEvent _capturedBunniesEvent;
		private SavedBunniesEvent _savedBunniesEvent;

		private readonly List<Transform> _availableSpots = new();
		private readonly List<GameObject> _capturedBunnies = new();

		private void Start()
		{
			foreach(Transform location in _smallBunniesLocations)
			{
				_availableSpots.Add(location);
			}

			_capturedBunniesEvent = new CapturedBunniesEvent(_playerId);
			_savedBunniesEvent = new SavedBunniesEvent(_playerId);

			EventBus.Register<PlayerBumpedEvent>(LoseBunnies);
		}

		public void Init(int playerId, Transform playerBase)
		{
			_playerBase = playerBase;
			_playerId = playerId;
			PlayerHelper.SetPlayerColor(gameObject, playerId);
		}

		private void Update()
		{
			_stunTimeRemainingSeconds = Mathf.Max(0, _stunTimeRemainingSeconds - Time.deltaTime);
		}

		public void AddBunny(GameObject capturedBunny)
		{
			if(!IsFull)
			{
				int spotIndex = Random.Range(0, _availableSpots.Count);
				Transform spot = _availableSpots[spotIndex];
				capturedBunny.transform.SetParent(spot, false);
				capturedBunny.transform.localPosition = Vector3.zero;
				capturedBunny.transform.localRotation = Quaternion.identity;
				capturedBunny.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
				_capturedBunniesCount++;
				_capturedBunnies.Add(capturedBunny);
				_availableSpots.RemoveAt(spotIndex);

				_capturedBunniesEvent.CapturedBunniesCount = _capturedBunniesCount;
				EventBus.Fire(_capturedBunniesEvent);
			}
		}

		private void LoseBunnies(PlayerBumpedEvent evt)
		{
			if(evt.PlayerId == _playerId && _capturedBunnies.Count > 0)
			{
				_stunTimeRemainingSeconds = _stunTimeSeconds;

				EventBus.Fire(new LostBunniesEvent(_capturedBunnies));

				_capturedBunnies.Clear();
				_availableSpots.Clear();

				foreach(Transform location in _smallBunniesLocations)
				{
					_availableSpots.Add(location);
				}

				_capturedBunniesCount = 0;

				_capturedBunniesEvent.CapturedBunniesCount = _capturedBunniesCount;
				EventBus.Fire(_capturedBunniesEvent);
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if(other.transform == _playerBase)
			{
				foreach(GameObject capturedBunny in _capturedBunnies)
				{
					capturedBunny.transform.SetParent(other.transform.parent, false);

					float sign = other.bounds.center.y < 0 ? -1 : 1;
					capturedBunny.transform.position = new Vector3(Random.Range(other.bounds.center.x - other.bounds.extents.x, other.bounds.center.x + other.bounds.extents.x), other.bounds.center.y - other.bounds.extents.y * sign, Random.Range(other.bounds.center.z - other.bounds.extents.z, other.bounds.center.z + other.bounds.extents.z));
					capturedBunny.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
					ParticleSystem particles = _deliveryParticles.Get(other.transform.parent);
					particles.transform.position = capturedBunny.transform.position;
					particles.Play();
					_savedBunniesCount++;
				}

				if(_capturedBunniesCount > 0)
				{
					_savedBunniesEvent.SavedBunniesCount = _savedBunniesCount;
					EventBus.Fire(_savedBunniesEvent);
					CameraShakeEvent cameraShakeEvent = new(0.1f * _capturedBunniesCount, new[] { _playerId });
					EventBus.Fire(cameraShakeEvent);
				}

				_capturedBunniesCount = 0;
				_capturedBunnies.Clear();
				_availableSpots.Clear();

				foreach(Transform location in _smallBunniesLocations)
				{
					_availableSpots.Add(location);
				}

				_capturedBunniesEvent.CapturedBunniesCount = _capturedBunniesCount;
				EventBus.Fire(_capturedBunniesEvent);
			}
		}

		private void OnDestroy()
		{
			_deliveryParticles.Dispose();
			EventBus.Unregister<PlayerBumpedEvent>(LoseBunnies);
		}
	}
}
