using BBR.GameLoop;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BBR
{
	public class BunnyPlayer : MonoBehaviour
	{
		[SerializeField] private Transform[] _smallBunniesLocations;

		private Transform _playerBase;

		private readonly List<Transform> _availableSpots = new();
		private readonly List<GameObject> _capturedBunnies = new();

		private void Start()
		{
			foreach(Transform location in _smallBunniesLocations)
			{
				_availableSpots.Add(location);
			}
		}

		public void Init(int playerId, Transform playerBase)
		{
			_playerBase = playerBase;
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
				_capturedBunnies.Add(capturedBunny);
				_availableSpots.RemoveAt(spotIndex);
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
				}
				_capturedBunnies.Clear();
				_availableSpots.Clear();
				foreach(Transform location in _smallBunniesLocations)
				{
					_availableSpots.Add(location);
				}
			}
		}
	}
}
