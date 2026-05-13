using UnityEngine;

namespace HamerSoft.Victoria.Core.Audio
{
    internal class RuntimeAudioSource : IAudioSource
    {
        private readonly AudioSource _audioSource;
        public bool IsPlaying => _audioSource.isPlaying;

        internal RuntimeAudioSource()
        {
            _audioSource = new GameObject("UnityPackage_AudioSource")
            {
                transform = { hideFlags = HideFlags.HideInHierarchy }
            }.AddComponent<AudioSource>();
        }

        /// <inheritdoc /> 
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

        /// <inheritdoc /> 
        public void Stop()
        {
            if (!_audioSource)
                return;
            _audioSource.Stop();
            _audioSource.clip = null;
        }

        /// <summary>
        /// Dispose (destroy) the audio source
        /// </summary>
        ///<remarks>stops the currently playing clip, but does not destroy it.</remarks>
        public void Dispose()
        {
            Stop();
            if (Application.isPlaying)
                Object.Destroy(_audioSource);
            else
                Object.DestroyImmediate(_audioSource);
        }
    }
}