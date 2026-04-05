using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pool.Pool
{
	[Serializable]
	public abstract class PoolBase<T> : IDisposable where T : Component
	{
		public int Count => _pool.Count;

		[SerializeField] private Transform _parent;
		[SerializeField] private GameObject _prefab;

		private readonly List<T> _pool = new();

		public T Get(Transform parent = null)
		{
			foreach(T item in _pool)
			{
				if(!item.gameObject.activeSelf)
				{
					item.transform.SetParent(parent);
					item.gameObject.SetActive(true);
					return item;
				}
			}

			return Instantiate();
		}

		public IEnumerable<T> Get(int count)
		{
			List<T> items = new(count);

			foreach(T item in _pool)
			{
				if(!item.gameObject.activeSelf)
				{
					item.gameObject.SetActive(true);
					items.Add(item);
				}
			}

			for(int i = 0; i < count - items.Count; i++)
			{
				items.Add(Instantiate());
			}

			return items;
		}

		private T Instantiate()
		{
			T item = Object.Instantiate(_prefab, _parent).GetComponent<T>();
			item.gameObject.SetActive(true);
			_pool.Add(item);
			return item;
		}

		public void ReturnAll()
		{
			foreach(T item in _pool)
			{
				Return(item);
			}
		}

		public void Return(IEnumerable<T> items)
		{
			foreach(T item in items)
			{
				Return(item);
			}
		}

		public virtual void Return(T item)
		{
			item.transform.SetParent(_parent);
		}

		public virtual void Dispose()
		{
			for(int i = _pool.Count - 1; i >= 0; i--)
			{
				if(_pool[i])
				{
					Object.Destroy(_pool[i].gameObject);
				}
			}

			_pool.Clear();
		}
	}
}
