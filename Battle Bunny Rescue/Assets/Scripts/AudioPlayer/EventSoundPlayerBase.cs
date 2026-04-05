using BBR.Events;
using UnityEngine;

namespace BBR.AudioPlayer
{
	public abstract class EventSoundPlayerBase<T> : MonoBehaviour
	{
		[SerializeField] private AudioHolder _sound;

		private void OnEnable()
		{
			EventBus.Register<T>(OnEventReceived);
		}

		private void OnEventReceived(T obj)
		{
			_sound.Play();
		}

		private void OnDisable()
		{
			EventBus.Unregister<T>(OnEventReceived);
		}
	}
}
