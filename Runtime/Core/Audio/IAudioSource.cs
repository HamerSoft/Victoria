using System;
using UnityEngine;

namespace HamerSoft.Victoria.Core.Audio
{
    /// <summary>
    /// AudioSource used for previewing audio data
    /// </summary>
    public interface IAudioSource : IDisposable
    {
        /// <summary>
        /// Flag whether the AudioSource is currently playing
        /// </summary>
        public bool IsPlaying { get; }

        /// <summary>
        /// Play the clip on the audio source
        /// </summary>
        /// <remarks>Stops the currently playing audio clip</remarks>
        /// <param name="clip">clip to play</param>
        public void Play(AudioClip clip);
        /// <summary>
        /// Stop the currently playing clip
        /// </summary>
        /// <remarks>If none is playing, this is a noop</remarks>
        public void Stop();
    }
}