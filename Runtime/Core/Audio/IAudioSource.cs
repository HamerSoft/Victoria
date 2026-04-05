using System;
using UnityEngine;

namespace HamerSoft.Victoria.Core.Audio
{
    public interface IAudioSource : IDisposable
    {
        public bool IsPlaying { get; }

        public void Play(AudioClip clip);
        public void Stop();
    }
}