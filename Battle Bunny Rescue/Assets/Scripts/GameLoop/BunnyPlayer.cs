using BBR.Events;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BBR.GameLoop
{
	public class BunnyPlayer : MonoBehaviour
	{
		[SerializeField] private Transform[] _smallBunniesLocations;

		private Transform _playerBase;
		private int _playerId;
		private int _capturedBunniesCount;
		private int _savedBunniesCount;
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
		}

		public void Init(int playerId, Transform playerBase)
		{
			_playerBase = playerBase;
			_playerId = playerId;
			PlayerHelper.SetPlayerColor(gameObject, playerId);
		}

		public void AddBunny(GameObject capturedBunny)
		{
			if(_availableSpots.Count > 0)
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

		private void OnTriggerEnter(Collider other)
		{
			if(other.transform == _playerBase)
			{
				foreach(GameObject capturedBunny in _capturedBunnies)
				{
					capturedBunny.transform.SetParent(other.transform.parent, false);
					capturedBunny.transform.position = new Vector3(Random.Range(other.bounds.center.x - other.bounds.extents.x, other.bounds.center.x + other.bounds.extents.x), other.bounds.center.y - other.bounds.extents.y, Random.Range(other.bounds.center.z - other.bounds.extents.z, other.bounds.center.z + other.bounds.extents.z));
					capturedBunny.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
					_savedBunniesCount++;
				}

				_capturedBunniesCount = 0;
				_capturedBunnies.Clear();
				_availableSpots.Clear();

				foreach(Transform location in _smallBunniesLocations)
				{
					_availableSpots.Add(location);
				}

				_savedBunniesEvent.SavedBunniesCount = _savedBunniesCount;
				EventBus.Fire(_savedBunniesEvent);

				_capturedBunniesEvent.CapturedBunniesCount = _capturedBunniesCount;
				EventBus.Fire(_capturedBunniesEvent);
			}
		}
	}
}