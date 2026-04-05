using System;
using System.Reflection;
using HamerSoft.Victoria.Core.Audio;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HamerSoft.Victoria.EditorAudio
{
    /// <summary>
    /// Methods can be found in the UnityEditor.AudioUtil Class
    /// You can decompile it, or view the source here and implement more methods
    /// https://github.com/jamesjlinden/unity-decompiled/blob/master/UnityEditor/UnityEditor/AudioUtil.cs
    /// </summary>
    public class EditorAudioSource : IAudioSource
    {
        private AudioClip _clip;
        protected static MethodInfo PlayMethod { get; private set; }
        protected static MethodInfo StopMethod { get; private set; }
        protected static MethodInfo IsPlayingMethod { get; private set; }
        protected static MethodInfo GetDurationMethod { get; private set; }
        protected static MethodInfo GetTargetPlatformSoundCompressionFormatMethod { get; private set; }
        protected static MethodInfo GetSoundCompressionFormatMethod { get; private set; }
        protected static MethodInfo GetSoundSizeMethod { get; private set; }
        protected static MethodInfo GetFrequencyMethod { get; private set; }
        protected static MethodInfo GetBitsPerSampleMethod { get; private set; }
        protected static MethodInfo GetBitRateMethod { get; private set; }
        protected static MethodInfo GetChannelCountMethod { get; private set; }
        protected static MethodInfo GetSampleCountMethod { get; private set; }
        protected static MethodInfo GetClipSamplePositionMethod { get; private set; }
        protected static MethodInfo GetClipPositionMethod { get; private set; }
        protected static MethodInfo StopAllClipsMethod { get; private set; }
        protected static MethodInfo LoopClipMethod { get; private set; }
        protected static MethodInfo ResumeClipMethod { get; private set; }
        protected static MethodInfo PauseClipMethod { get; private set; }
        public bool IsPlaying => IsPlayingClip();
        public bool IsLooping { get; private set; }

        public EditorAudioSource()
        {
            IsLooping = false;
            PlayMethod = GetMethod("PlayPreviewClip", PlayMethod);
            StopMethod = GetMethod("StopAllPreviewClips", StopMethod);
            IsPlayingMethod = GetMethod("IsPreviewClipPlaying", IsPlayingMethod);
            PauseClipMethod = GetMethod("PausePreviewClip", PauseClipMethod);
            ResumeClipMethod = GetMethod("ResumePreviewClip", ResumeClipMethod);
            LoopClipMethod = GetMethod("LoopPreviewClip", LoopClipMethod);
            StopAllClipsMethod = GetMethod("StopAllPreviewClips", StopAllClipsMethod);
            GetClipPositionMethod = GetMethod("GetPreviewClipPosition", GetClipPositionMethod);
            GetClipSamplePositionMethod = GetMethod("GetPreviewClipSamplePosition", GetClipSamplePositionMethod);
            GetSampleCountMethod = GetMethod("GetSampleCount", GetSampleCountMethod);
            GetChannelCountMethod = GetMethod("GetChannelCount", GetChannelCountMethod);
            GetBitRateMethod = GetMethod("GetBitRate", GetBitRateMethod);
            GetBitsPerSampleMethod = GetMethod("GetBitsPerSample", GetBitsPerSampleMethod);
            GetFrequencyMethod = GetMethod("GetFrequency", GetFrequencyMethod);
            GetSoundSizeMethod = GetMethod("GetSoundSize", GetSoundSizeMethod);
            GetSoundCompressionFormatMethod = GetMethod("GetSoundCompressionFormat", GetSoundCompressionFormatMethod);
            GetTargetPlatformSoundCompressionFormatMethod = GetMethod("GetTargetPlatformSoundCompressionFormat",
                GetTargetPlatformSoundCompressionFormatMethod);
            GetDurationMethod = GetMethod("GetDuration", GetDurationMethod);
        }

        private static MethodInfo GetMethod(string methodName, MethodInfo target)
        {
            if (target != null)
                return target;
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            return audioUtilClass.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Public
            );
        }

        public void Play(int startSample = 0, bool loop = false)
        {
            IsLooping = loop;
            PlayMethod.Invoke(
                null,
                new object[]
                {
                    _clip,
                    startSample,
                    loop
                }
            );
        }

        public void Play(AudioClip clip)
        {
            _clip = clip;
            Play();
        }

        public void SetClip(AudioClip clip)
        {
            if (IsPlaying)
                Stop();

            _clip = clip;
        }

        public void Stop()
        {
            if (_clip == null)
                return;
            StopMethod.Invoke(
                null,
                Array.Empty<object>()
            );
        }

        public bool IsPlayingClip()
        {
            if (_clip == null)
                return false;
            return (bool)IsPlayingMethod.Invoke(
                null,
                Array.Empty<object>()
            );
        }

        public void PauseClip()
        {
            if (_clip == null)
                return;

            PauseClipMethod.Invoke(
                null,
                Array.Empty<object>()
            );
        }

        public void ResumeClip()
        {
            if (_clip == null)
                return;
            ResumeClipMethod.Invoke(
                null,
                Array.Empty<object>()
            );
        }

        public void LoopClip(bool on)
        {
            IsLooping = on;
            if (_clip == null)
                return;

            LoopClipMethod.Invoke(
                null,
                new object[]
                {
                    on
                }
            );
        }

        public float GetClipPosition()
        {
            if (_clip == null)
                return 0;
            return (float)GetClipPositionMethod.Invoke(
                null,
                Array.Empty<object>()
            );
        }

        public int GetClipSamplePosition()
        {
            if (_clip == null)
                return 0;
            return (int)GetClipSamplePositionMethod.Invoke(
                null,
                Array.Empty<object>()
            );
        }

        public int GetSampleCount()
        {
            if (_clip == null)
                return 0;
            return (int)GetSampleCountMethod.Invoke(
                null,
                new object[]
                {
                    _clip,
                }
            );
        }

        public int GetChannelCount()
        {
            if (_clip == null)
                return 0;
            return (int)GetChannelCountMethod.Invoke(
                null,
                new object[]
                {
                    _clip,
                }
            );
        }

        public int GetBitRate()
        {
            if (_clip == null)
                return 0;
            return (int)GetBitRateMethod.Invoke(
                null,
                new object[]
                {
                    _clip,
                }
            );
        }

        public int GetBitsPerSample()
        {
            if (_clip == null)
                return 0;
            return (int)GetBitsPerSampleMethod.Invoke(
                null,
                new object[]
                {
                    _clip,
                }
            );
        }

        public int GetFrequency()
        {
            if (_clip == null)
                return 0;
            return (int)GetFrequencyMethod.Invoke(
                null,
                new object[]
                {
                    _clip,
                }
            );
        }

        public int GetSoundSize()
        {
            if (_clip == null)
                return 0;
            return (int)GetSoundSizeMethod.Invoke(
                null,
                new object[]
                {
                    _clip,
                }
            );
        }

        public AudioCompressionFormat GetSoundCompressionFormat()
        {
            if (_clip == null)
                return AudioCompressionFormat.Vorbis;
            return (AudioCompressionFormat)GetSoundCompressionFormatMethod.Invoke(
                null,
                new object[]
                {
                    _clip,
                }
            );
        }

        public AudioCompressionFormat GetTargetPlatformSoundCompressionFormat()
        {
            if (_clip == null)
                return AudioCompressionFormat.Vorbis;
            return (AudioCompressionFormat)GetTargetPlatformSoundCompressionFormatMethod.Invoke(
                null,
                new object[]
                {
                    _clip,
                }
            );
        }

        public float GetDuration()
        {
            if (_clip == null)
                return 0;
            return Convert.ToSingle(GetDurationMethod.Invoke(
                null,
                new object[]
                {
                    _clip,
                }
            ));
        }

        public void Dispose()
        {
            Stop();
        }

        public static void StopAllClips()
        {
            if (StopAllClipsMethod == null)
                StopAllClipsMethod = GetMethod("StopAllClips", StopAllClipsMethod);
            StopAllClipsMethod.Invoke(
                null, null
            );
        }
    }

    public class EditorAudioSourceTests
    {
        private MockAudioSource _audioSource;

        private class MockAudioSource : EditorAudioSource
        {
            public MockAudioSource(AudioClip clip)
            {
                SetClip(clip);
            }

            public MethodInfo GetStopMethod()
            {
                return StopMethod;
            }

            public MethodInfo GetPlayMethod()
            {
                return PlayMethod;
            }

            public MethodInfo GetIsPlayingMethod()
            {
                return IsPlayingMethod;
            }

            public MethodInfo GetGetDurationMethod()
            {
                return GetDurationMethod;
            }

            public MethodInfo GetGetTargetPlatformSoundCompressionFormatMethod()
            {
                return GetTargetPlatformSoundCompressionFormatMethod;
            }

            public MethodInfo GetGetSoundCompressionFormatMethod()
            {
                return GetSoundCompressionFormatMethod;
            }

            public MethodInfo GetGetSoundSizeMethod()
            {
                return GetSoundSizeMethod;
            }

            public MethodInfo GetGetFrequencyMethod()
            {
                return GetFrequencyMethod;
            }

            public MethodInfo GetGetBitsPerSampleMethod()
            {
                return GetBitsPerSampleMethod;
            }

            public MethodInfo GetGetBitRateMethod()
            {
                return GetBitRateMethod;
            }

            public MethodInfo GetGetChannelCountMethod()
            {
                return GetChannelCountMethod;
            }

            public MethodInfo GetGetSampleCountMethod()
            {
                return GetSampleCountMethod;
            }

            public MethodInfo GetGetClipSamplePositionMethod()
            {
                return GetClipSamplePositionMethod;
            }

            public MethodInfo GetGetClipPositionMethod()
            {
                return GetClipPositionMethod;
            }

            public MethodInfo GetStopAllClipsMethod()
            {
                return StopAllClipsMethod;
            }

            public MethodInfo GetLoopClipMethod()
            {
                return LoopClipMethod;
            }

            public MethodInfo GetResumeClipMethod()
            {
                return ResumeClipMethod;
            }

            public MethodInfo GetPauseClipMethod()
            {
                return PauseClipMethod;
            }
        }

        [SetUp]
        public void Setup()
        {
            _audioSource = new MockAudioSource(Resources.Load<AudioClip>("EditorAudio/Awesome"));
        }

        [Test]
        public void WhenConstructed_TheEditorAudioSource_DoesNotThrowExceptions()
        {
            Assert.DoesNotThrow(() => { new MockAudioSource(Resources.Load<AudioClip>("EditorAudio/Awesome")); });
        }

        [Test]
        public void When_EDITORAudioSource_IsConstructed_TheMethodsAre_NOTNULL()
        {
            Assert.NotNull(_audioSource.GetPlayMethod());
            Assert.NotNull(_audioSource.GetStopMethod());
            Assert.NotNull(_audioSource.GetIsPlayingMethod());
            Assert.NotNull(_audioSource.GetGetDurationMethod());
            Assert.NotNull(_audioSource.GetGetTargetPlatformSoundCompressionFormatMethod());
            Assert.NotNull(_audioSource.GetGetSoundCompressionFormatMethod());
            Assert.NotNull(_audioSource.GetGetSoundSizeMethod());
            Assert.NotNull(_audioSource.GetGetFrequencyMethod());
            Assert.NotNull(_audioSource.GetGetBitsPerSampleMethod());
            Assert.NotNull(_audioSource.GetGetBitRateMethod());
            Assert.NotNull(_audioSource.GetGetChannelCountMethod());
            Assert.NotNull(_audioSource.GetGetSampleCountMethod());
            Assert.NotNull(_audioSource.GetGetClipSamplePositionMethod());
            Assert.NotNull(_audioSource.GetGetClipPositionMethod());
            Assert.NotNull(_audioSource.GetStopAllClipsMethod());
            Assert.NotNull(_audioSource.GetLoopClipMethod());
            Assert.NotNull(_audioSource.GetResumeClipMethod());
            Assert.NotNull(_audioSource.GetPauseClipMethod());
        }

        [Test]
        public void Play_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.Play());
        }

        [Test]
        public void Stop_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() =>
            {
                _audioSource.Play();
                _audioSource.Stop();
            });
        }

        [Test]
        public void IsPlayingMethod_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() =>
            {
                _audioSource.Play();
                Assert.True(_audioSource.IsPlaying);
            });
        }

        [Test]
        public void GetDuration_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetDuration());
        }

        [Test]
        public void GetTargetPlatformSoundCompressionFormat_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetTargetPlatformSoundCompressionFormat());
        }

        [Test]
        public void GetSoundCompressionFormat_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetSoundCompressionFormat());
        }

        [Test]
        public void GetSoundSize_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetSoundSize());
        }

        [Test]
        public void GetFrequency_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetFrequency());
        }

        [Test]
        public void GetBitsPerSample_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetBitsPerSample());
        }

        [Test]
        public void GetBitRate_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetBitRate());
        }

        [Test]
        public void GetChannelCount_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetChannelCount());
        }

        [Test]
        public void GetSampleCount_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetSampleCount());
        }

        [Test]
        public void GetClipSamplePosition_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetClipSamplePosition());
        }

        [Test]
        public void GetClipPosition_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetClipPosition());
        }

        [Test]
        public void StopAllAudioClips_DoesNotThrowException()
        {
            Assert.DoesNotThrow(EditorAudioSource.StopAllClips);
        }

        [Test]
        public void LoopClip_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() =>
            {
                _audioSource.Play();
                _audioSource.LoopClip(true);
                Assert.True(_audioSource.IsLooping);
            });
        }

        [Test]
        public void Resume_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() =>
            {
                _audioSource.Play();
                _audioSource.PauseClip();
                _audioSource.ResumeClip();
            });
        }

        [Test]
        public void Pause_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() =>
            {
                _audioSource.Play();
                _audioSource.PauseClip();
            });
        }
    }
}