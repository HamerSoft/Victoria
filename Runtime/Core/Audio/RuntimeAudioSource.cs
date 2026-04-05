using UnityEngine;

namespace HamerSoft.Victoria.Core.Audio
{
    public class RuntimeAudioSource : IAudioSource
    {
        private readonly AudioSource _audioSource;
        public bool IsPlaying => _audioSource.isPlaying;

        public RuntimeAudioSource()
        {
            _audioSource = new GameObject("UnityPackage_AudioSource")
            {
                transform = { hideFlags = HideFlags.HideInHierarchy }
            }.AddComponent<AudioSource>();
        }

        public void Play(AudioClip clip)
        {
            Stop();
            if (!clip)
            {
                Debug.LogWarning("Cannot preview AudioClip, it is null.");
                return;
            }

            _audioSource.clip = clip;
            _audioSource.Play();
        }

        public void Stop()
        {
            _audioSource.Stop();
            _audioSource.clip = null;
        }

        public void Dispose()
        {
            Stop();
            Object.Destroy(_audioSource);
        }
    }
}