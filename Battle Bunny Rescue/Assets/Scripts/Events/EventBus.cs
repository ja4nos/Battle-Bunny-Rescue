using System;
using System.Collections.Generic;
using UnityEngine;

namespace BBR.Events
{
	public static class EventBus
	{
		private static readonly Dictionary<Type, List<Delegate>> _callbacks = new();

		public static void Register<T>(Action<T> callback)
		{
			if(!_callbacks.ContainsKey(typeof(T)))
			{
				_callbacks.Add(typeof(T), new List<Delegate>());
			}

			if(_callbacks[typeof(T)].Contains(callback))
			{
				Debug.LogError($"Callback {callback} already registered for type {typeof(T)}");
				return;
			}

			_callbacks[typeof(T)].Add(callback);
		}

		public static void Unregister<T>(Action<T> callback)
		{
			if(!_callbacks.TryGetValue(typeof(T), out List<Delegate> callbacks))
			{
				Debug.LogWarning(
					$"Trying to unregister callback {callback} for type {typeof(T)} but it was not registered");
				return;
			}

			if(!callbacks.Contains(callback))
			{
				Debug.LogWarning(
					$"Trying to unregister callback {callback} for type {typeof(T)} but it was not registered");
			}

			callbacks.Remove(callback);
		}

		public static void Fire<T>(T eventData)
		{
			if(!_callbacks.TryGetValue(typeof(T), out List<Delegate> callbacks))
			{
				return;
			}

			foreach(Delegate callback in callbacks)
			{
				callback.DynamicInvoke(eventData);
			}
		}
	}
}
