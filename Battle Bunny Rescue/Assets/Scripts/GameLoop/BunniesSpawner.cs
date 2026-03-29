using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace BBR
{
	[RequireComponent(typeof(SphereCollider))]
	public class BunniesSpawner : MonoBehaviour
	{
		[SerializeField] private GameObject _bunnyPrefab;
		[SerializeField] [Min(1)] private int _maxAmountOfBunnies = 100;
		[SerializeField] [Min(0.1f)] private float _spawnRateSeconds = 1f;
		[SerializeField] [Min(1)] private int _spawnRateAmount = 2;

		private int _spawnedBunnies;
		private SphereCollider _collider;
		private float _elapsedTime;
		private ObjectPool<GameObject> _pool;

		private readonly Queue<GameObject> _availableBunnies = new();

		private void Start()
		{
			_collider = GetComponent<SphereCollider>();

			_pool = new ObjectPool<GameObject>(
				createFunc: SpawnBunny,
				actionOnGet: OnGet,
				actionOnRelease: OnRelease,
				actionOnDestroy: OnDestroyItem,
				maxSize: _maxAmountOfBunnies
			);
		}

		private void Update()
		{
			_elapsedTime += Time.deltaTime;
			if(_elapsedTime > _spawnRateSeconds)
			{
				if(_availableBunnies.Count < _maxAmountOfBunnies)
				{
					for(int i = 0; i < _spawnRateAmount; i++)
					{
						_pool.Get(out GameObject bunny);
						_availableBunnies.Enqueue(bunny);
					}
				}

				_elapsedTime = 0;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if(other.CompareTag("Player")
				&& _availableBunnies.TryDequeue(out GameObject capturedBunny)
				&& other.TryGetComponent(out BunnyPlayer player))
			{
				player.AddBunny(capturedBunny);
			}
		}

		private GameObject SpawnBunny()
		{
			GameObject bunny = Instantiate(_bunnyPrefab, transform);
			bunny.name = "Bunny";
			bunny.SetActive(false);
			return bunny;
		}

		private void OnGet(GameObject bunny)
		{
			bunny.transform.position = GetRandomSpawnPosition();
			bunny.SetActive(true);
		}

		private static void OnRelease(GameObject bunny)
		{
			bunny.SetActive(false);
		}

		private static void OnDestroyItem(GameObject bunny)
		{
			Destroy(bunny);
		}

		private Vector3 GetRandomSpawnPosition()
		{
			float radius = _collider.radius * transform.localScale.x;
			Vector3 center = transform.position + _collider.center;

			float theta = Random.Range(0f, Mathf.PI * 0.5f);
			float phi = Random.Range(0f, Mathf.PI * 2f);

			float sinTheta = Mathf.Sin(theta);
			Vector3 candidate = center + new Vector3(
				sinTheta * Mathf.Cos(phi),
				Mathf.Cos(theta),
				sinTheta * Mathf.Sin(phi)
			) * Random.Range(0f, radius);

			return candidate;
		}

		private void OnDestroy()
		{
			_pool.Dispose();
		}
	}
}
