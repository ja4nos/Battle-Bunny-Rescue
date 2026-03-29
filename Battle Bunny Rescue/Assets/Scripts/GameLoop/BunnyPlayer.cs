using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BBR
{
	public class BunnyPlayer : MonoBehaviour
	{
		[SerializeField] private Transform[] _smallBunniesLocations;

		private readonly List<Transform> _availableSpots = new();

		private void Start()
		{
			foreach(Transform location in _smallBunniesLocations)
			{
				_availableSpots.Add(location);
			}
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
				_availableSpots.RemoveAt(spotIndex);
			}
		}
	}
}