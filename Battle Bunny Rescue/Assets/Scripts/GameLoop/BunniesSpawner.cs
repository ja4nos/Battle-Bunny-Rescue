using BBR.Events;
using BBR.Movement;
using Pool.Pool;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace BBR.GameLoop
{
	public class BunniesSpawner : MonoBehaviour
	{
		[SerializeField] private GameObject _bunnyPrefab;
		[SerializeField] [Min(1)] private int _maxAmountOfBunnies = 100;
		[SerializeField] [Min(0.1f)] private float _spawnRateSeconds = 1f;
		[SerializeField] [Min(1)] private int _spawnRateAmount = 2;
		[SerializeField] [Min(0.1f)] private float _radius = 75f;
		[SerializeField] [Range(0.1f, 1f)] private float _minSize = 0.1f;
		[SerializeField] [Range(0.1f, 1f)] private float _maxSize = 0.3f;
		[SerializeField] private ParticlePool _bunnyRescueParticlePool;

		[Header("Event Camera")] [SerializeField]
		private Vector3 _cameraOffset = new(0f, 10f, 0f);

		[SerializeField] private float _eventDurationSeconds = 2f;

		private int _spawnedBunnies;
		private float _elapsedTime;
		private ObjectPool<BunnyMovementRandom> _pool;

		private readonly Queue<BunnyMovementRandom> _availableBunnies = new();

		private void Start()
		{
			_pool = new ObjectPool<BunnyMovementRandom>(
				createFunc: SpawnBunny,
				actionOnGet: OnGet,
				actionOnRelease: OnRelease,
				actionOnDestroy: OnDestroyItem,
				maxSize: _maxAmountOfBunnies
			);

			EventBus.Register<LostBunniesEvent>(OnLostBunnies);
		}

		private void Update()
		{
			_elapsedTime += Time.deltaTime;
			if(_elapsedTime > _spawnRateSeconds)
			{
				if(_availableBunnies.Count < _maxAmountOfBunnies)
				{
					for(int i = 0; i < _spawnRateAmount && _availableBunnies.Count < _maxAmountOfBunnies; i++)
					{
						_pool.Get(out BunnyMovementRandom bunny);
						_availableBunnies.Enqueue(bunny);
					}

					//TODO: Disabled for now, we can fire it again once we have powerups or enough bunnies in a base, etc.
					// if(_availableBunnies.Count == _maxAmountOfBunnies)
					// {
					// 	CameraShowEvent cameraShowEvent = new(transform.position + _cameraOffset, _eventDurationSeconds);
					// 	EventBus.Fire(cameraShowEvent);
					// }
				}

				_elapsedTime = 0;
			}
		}

		private void RescueBunny(BunnyMovementRandom capturedBunny, Collider other)
		{
			BunnyPlayer player = other.GetComponentInParent<BunnyPlayer>();

			if(player && !player.IsStunned && !player.IsFull)
			{
				BunnyMovementRandom bunny = Instantiate(capturedBunny);
				bunny.VisualTransform.localPosition = Vector3.zero;
				ParticleSystem rescueParticle = _bunnyRescueParticlePool.Get();
				rescueParticle.transform.position = player.transform.position;
				rescueParticle.Play();
				Destroy(bunny);
				Destroy(bunny.GetComponent<Collider>());
				Destroy(bunny.GetComponent<Rigidbody>());
				player.AddBunny(bunny.gameObject);

				_pool.Release(capturedBunny);
			}
		}

		private BunnyMovementRandom SpawnBunny()
		{
			GameObject bunny = Instantiate(_bunnyPrefab, transform);
			bunny.name = "Bunny";
			float size = Random.Range(_minSize, _maxSize);
			bunny.transform.localScale = new Vector3(size, size, size);
			BunnyMovementRandom bunnyMovement = bunny.GetComponent<BunnyMovementRandom>();
			bunnyMovement.OnBunnyPlayerCollision += RescueBunny;
			bunny.SetActive(false);
			return bunnyMovement;
		}

		private void OnGet(BunnyMovementRandom bunny)
		{
			bunny.transform.position = GetRandomSpawnPosition();
			bunny.gameObject.SetActive(true);
		}

		private void OnRelease(BunnyMovementRandom bunny)
		{
			bunny.gameObject.SetActive(false);
			bunny.transform.SetParent(transform);
			_elapsedTime = 0f;
		}

		private static void OnDestroyItem(BunnyMovementRandom bunny)
		{
			Destroy(bunny.gameObject);
		}

		private Vector3 GetRandomSpawnPosition()
		{
			float radius = _radius * transform.localScale.x;

			float theta = Random.Range(0f, Mathf.PI * 0.5f);
			float phi = Random.Range(0f, Mathf.PI * 2f);

			float sinTheta = Mathf.Sin(theta);
			Vector3 candidate = transform.position + new Vector3(
				sinTheta * Mathf.Cos(phi),
				0,
				sinTheta * Mathf.Sin(phi)
			) * Random.Range(0f, radius);

			return candidate;
		}

		private void OnLostBunnies(LostBunniesEvent evt)
		{
			foreach(GameObject lostBunny in evt.LostBunnies)
			{
				_pool.Get(out BunnyMovementRandom bunny);
				_availableBunnies.Enqueue(bunny);
				bunny.transform.position = lostBunny.transform.position;
				Destroy(lostBunny);
			}
		}

		private void OnDestroy()
		{
			EventBus.Unregister<LostBunniesEvent>(OnLostBunnies);
			_pool.Dispose();
		}
	}
}
