using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace BBR.AudioPlayer
{
	public class AudioPlayer : MonoBehaviour
	{
		[SerializeField] private int _initializeSources = 4;
		[SerializeField] [Range(-80, 20)] private float _minimumVolume = -80;
		[SerializeField] [Range(-80, 20)] private float _maximumVolume;
		[SerializeField] private float _volumeChangeSpeed = 7;
		[SerializeField] private GameObject _sfxSourceTemplate;

		// Hacky hack...
		private static GameObject _sfxTemplate;
		private static AudioMixer _mixer;
		private static AudioMixerGroup _sfxMixer;
		private static readonly List<AudioSource> _sources = new();
		private static AudioSource _musicSource;
		private static GameObject _thisObject;
		private static float _minVolume = -80;
		private static float _maxVolume = 20;
		private static float _volumeSpeed = 7;

		private static readonly Dictionary<string, Tween> _tweens = new();

		#region Unity Methods

		private void Awake()
		{
			Set(Resources.Load<AudioMixer>("Mixer"), _minimumVolume, _maximumVolume, _volumeChangeSpeed);
		}

		private void OnDisable()
		{
			foreach(Tween tween in _tweens.Values)
			{
				if(tween != null && tween.IsActive())
				{
					tween.Kill();
				}
			}

			if(_musicSource)
			{
				Destroy(_musicSource);
			}

			AudioSource[] sources = _thisObject.GetComponents<AudioSource>();

			foreach(AudioSource source in sources)
			{
				Destroy(source.gameObject);
			}

			_sfxTemplate = null;
			_musicSource = null;
			_sources.Clear();
			_tweens.Clear();
		}

		#endregion

		/// <summary>
		/// Initialized the AudioPlayer. Use it to override default values
		/// </summary>
		/// <param name="mixer">New mixer to use</param>
		/// <param name="minVolume">Minimum volume to be set in the mixer</param>
		/// <param name="maxVolume">Maximum volume to be set in the mixer</param>
		/// <param name="volumeSpeed">How fast the volume should increase/decrease when music stops</param>
		/// <param name="musicMixerGroup">Mixer group name which controls the music volume</param>
		/// <param name="sfxMixerGroup">Mixer group name which controls the SFX</param>
		private void Set(AudioMixer mixer = null, float minVolume = -80, float maxVolume = 20, float volumeSpeed = 5,
			string musicMixerGroup = "Music", string sfxMixerGroup = "SFX")
		{
			_mixer = mixer;
			_minVolume = minVolume;
			_maxVolume = maxVolume;
			_volumeSpeed = volumeSpeed;
			_thisObject = gameObject;
			_sfxTemplate = _sfxSourceTemplate;

			if(!_mixer)
			{
				Debug.LogError($"Could not find mixer '{mixer}' for audio player!", this);
				return;
			}

			AudioMixerGroup[] sfxGroups = _mixer.FindMatchingGroups(sfxMixerGroup);

			if(sfxGroups.Length > 0)
			{
				_sfxMixer = sfxGroups[0];

				for(int i = 0; i < _initializeSources; i++)
				{
					AudioSource source = Instantiate(_sfxTemplate, _thisObject.transform).GetComponent<AudioSource>();
					source.outputAudioMixerGroup = sfxGroups[0];
					_sources.Add(source);
				}
			}
			else
			{
				Debug.LogWarning(
					$"Mixer '{_mixer}' does not have an sfx group '{sfxMixerGroup}'. Volume settings won't work!",
					_mixer);
			}

			_musicSource = _thisObject.AddComponent<AudioSource>();

			AudioMixerGroup[] musicGroups = _mixer.FindMatchingGroups(musicMixerGroup);

			if(musicGroups.Length > 0)
			{
				_musicSource.outputAudioMixerGroup = musicGroups[0];
			}
			else
			{
				Debug.LogWarning(
					$"Mixer '{_mixer}' does not have a music group '{musicMixerGroup}'. Volume settings won't work!",
					_mixer);
			}
		}

		/// <summary>
		/// Plays a clip using an individual audio source. Creates one if all sources are currently busy
		/// </summary>
		/// <param name="clip">Clip to play</param>
		/// <param name="minPitch">Minimum pitch</param>
		/// <param name="maxPitch">Maximum pitch</param>
		/// <param name="volume">Volume of the sound</param>
		public static void PlayOneShot(AudioClip clip, float minPitch = 1, float maxPitch = 1, float volume = 1)
		{
			foreach(AudioSource s in _sources)
			{
				if(s.isPlaying)
				{
					continue;
				}

				s.volume = volume;
				s.pitch = Random.Range(minPitch, maxPitch);
				s.PlayOneShot(clip);
				return;
			}

			AudioSource source = Instantiate(_sfxTemplate, _thisObject.transform).GetComponent<AudioSource>();
			source.outputAudioMixerGroup = _sfxMixer;
			source.loop = false;
			source.volume = volume;
			source.pitch = Random.Range(minPitch, maxPitch);
			source.PlayOneShot(clip);
			_sources.Add(source);
		}

		/// <summary>
		/// Plays a music in a dedicated audio source. If volume label is set fades out/in the track.
		/// </summary>
		/// <param name="clip">Track to play</param>
		/// <param name="volumeLabel">Exposed volume parameter label in the mixer</param>
		/// <param name="volume">Volume to play the next track at</param>
		/// <param name="isLooping">Should the track loop?</param>
		public static void PlayMusic(AudioClip clip, string volumeLabel = "", float volume = -1, bool isLooping = true)
		{
			if(string.IsNullOrEmpty(volumeLabel))
			{
				_musicSource.clip = clip;
				_musicSource.loop = isLooping;
				_musicSource.Play();
				return;
			}

			if(volume < 0)
			{
				_mixer.GetFloat(volumeLabel, out volume);
			}

			SetVolume(volumeLabel, 0, callback: () =>
			{
				_musicSource.clip = clip;
				_musicSource.loop = isLooping;
				_musicSource.Play();
				SetVolume(volumeLabel, (volume - _minVolume) / (_maxVolume - _minVolume));
			});
		}

		/// <summary>
		/// Stops the current music. If the label is set will fade out before stopping
		/// </summary>
		/// <param name="volumeLabel">Exposed volume parameter label in the mixer</param>
		public static void StopMusic(string volumeLabel = "")
		{
			if(string.IsNullOrEmpty(volumeLabel) || !_mixer.GetFloat(volumeLabel, out float currentVolume))
			{
				_musicSource.Stop();
				_musicSource.clip = null;
				return;
			}

			SetVolume(volumeLabel, 0, callback: () =>
			{
				_musicSource.Stop();
				_musicSource.clip = null;
				SetVolume(volumeLabel, currentVolume, true);
			});
		}

		/// <summary>
		/// Sets the volume for a given label.
		/// </summary>
		/// <param name="volumeLabel">Label of an exposed variable on the mixer</param>
		/// <param name="volume">Desired volume level between 0-1</param>
		/// <param name="instant">Should the volume be changed instantly or gradually</param>
		/// <param name="callback">Invoked after the volume is set to desired level</param>
		public static void SetVolume(string volumeLabel, float volume, bool instant = false, Action callback = null)
		{
			float range = _maxVolume - _minVolume;

			if(instant)
			{
				_mixer.SetFloat(volumeLabel, _minVolume + range * volume);
				callback?.Invoke();
				return;
			}

			if(_tweens.TryGetValue(volumeLabel, out Tween tween))
			{
				if(tween != null && tween.IsActive())
				{
					tween.Kill();
				}
			}
			else
			{
				_tweens.Add(volumeLabel, null);
			}

			_mixer.GetFloat(volumeLabel, out float currentVolume);
			_tweens[volumeLabel] = DOVirtual.Float(currentVolume, _minVolume + range * volume, _volumeSpeed, val => _mixer.SetFloat(volumeLabel, val));
		}
	}
}
