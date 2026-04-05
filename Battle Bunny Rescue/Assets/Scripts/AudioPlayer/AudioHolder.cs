using UnityEngine;

namespace BBR.AudioPlayer
{
	[CreateAssetMenu(fileName = "AudioHolder", menuName = "BBR/AudioHolder", order = 0)]
	public class AudioHolder : ScriptableObject
	{
		[SerializeField] private AudioClip[] _audioClips;
		[SerializeField] private float _minPitch = 1;
		[SerializeField] private float _maxPitch = 1;
		[SerializeField] private float _volume = 1;

		public void Play()
		{
			AudioClip clip = _audioClips[Random.Range(0, _audioClips.Length)];
			AudioPlayer.PlayOneShot(clip, _minPitch, _maxPitch, _volume);
		}
	}
}
