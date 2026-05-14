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

        /// <summary>
        /// Initializes a new instance of <see cref="EditorAudioSource"/> and resolves all
        /// <c>UnityEditor.AudioUtil</c> methods via reflection.
        /// </summary>
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

        /// <summary>
        /// Lazily resolves a static public method from <c>UnityEditor.AudioUtil</c> by name.
        /// Returns <paramref name="target"/> unchanged if it is already resolved.
        /// </summary>
        /// <param name="methodName">The name of the static public method to find on <c>AudioUtil</c>.</param>
        /// <param name="target">An existing <see cref="MethodInfo"/> to reuse; pass <c>null</c> to force resolution.</param>
        /// <returns>
        /// The resolved <see cref="MethodInfo"/>, or <paramref name="target"/> if it was already non-null.
        /// </returns>
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

        /// <summary>
        /// Plays the currently assigned clip from the given sample offset with an optional loop.
        /// </summary>
        /// <param name="startSample">The sample index at which playback begins. Defaults to <c>0</c>.</param>
        /// <param name="loop">Whether the clip should loop. Defaults to <c>false</c>.</param>
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

        /// <summary>
        /// Assigns <paramref name="clip"/> as the active clip and immediately starts playback from the beginning.
        /// </summary>
        /// <param name="clip">The <see cref="AudioClip"/> to assign and play.</param>
        public void Play(AudioClip clip)
        {
            _clip = clip;
            Play();
        }

        /// <summary>
        /// Sets the active clip. Stops any current playback before switching clips.
        /// </summary>
        /// <param name="clip">The <see cref="AudioClip"/> to set as the active clip.</param>
        public void SetClip(AudioClip clip)
        {
            if (IsPlaying)
                Stop();

            _clip = clip;
        }

        /// <summary>
        /// Stops playback of the active clip. Does nothing if no clip is assigned.
        /// </summary>
        public void Stop()
        {
            if (_clip == null)
                return;
            StopMethod.Invoke(
                null,
                Array.Empty<object>()
            );
        }

        /// <summary>
        /// Returns whether the active clip is currently playing.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a clip is assigned and currently playing; <c>false</c> if no clip is assigned
        /// or playback has stopped.
        /// </returns>
        public bool IsPlayingClip()
        {
            if (_clip == null)
                return false;
            return (bool)IsPlayingMethod.Invoke(
                null,
                Array.Empty<object>()
            );
        }

        /// <summary>
        /// Pauses playback of the active clip. Does nothing if no clip is assigned.
        /// </summary>
        public void PauseClip()
        {
            if (_clip == null)
                return;

            PauseClipMethod.Invoke(
                null,
                Array.Empty<object>()
            );
        }

        /// <summary>
        /// Resumes playback of a previously paused clip. Does nothing if no clip is assigned.
        /// </summary>
        public void ResumeClip()
        {
            if (_clip == null)
                return;
            ResumeClipMethod.Invoke(
                null,
                Array.Empty<object>()
            );
        }

        /// <summary>
        /// Enables or disables looping for the active clip.
        /// </summary>
        /// <param name="on"><c>true</c> to enable looping; <c>false</c> to disable it.</param>
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

        /// <summary>
        /// Gets the current playback position of the active clip in seconds.
        /// </summary>
        /// <returns>
        /// The playback position in seconds, or <c>0</c> if no clip is assigned.
        /// </returns>
        public float GetClipPosition()
        {
            if (_clip == null)
                return 0;
            return (float)GetClipPositionMethod.Invoke(
                null,
                Array.Empty<object>()
            );
        }

        /// <summary>
        /// Gets the current playback position of the active clip in samples.
        /// </summary>
        /// <returns>
        /// The playback position as a sample index, or <c>0</c> if no clip is assigned.
        /// </returns>
        public int GetClipSamplePosition()
        {
            if (_clip == null)
                return 0;
            return (int)GetClipSamplePositionMethod.Invoke(
                null,
                Array.Empty<object>()
            );
        }

        /// <summary>
        /// Gets the total number of samples in the active clip.
        /// </summary>
        /// <returns>
        /// The sample count, or <c>0</c> if no clip is assigned.
        /// </returns>
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

        /// <summary>
        /// Gets the number of audio channels in the active clip.
        /// </summary>
        /// <returns>
        /// The channel count (e.g. <c>1</c> for mono, <c>2</c> for stereo), or <c>0</c> if no clip is assigned.
        /// </returns>
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

        /// <summary>
        /// Gets the bit rate of the active clip in bits per second.
        /// </summary>
        /// <returns>
        /// The bit rate in bps, or <c>0</c> if no clip is assigned.
        /// </returns>
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

        /// <summary>
        /// Gets the bit depth (bits per sample) of the active clip.
        /// </summary>
        /// <returns>
        /// The bits-per-sample depth, or <c>0</c> if no clip is assigned.
        /// </returns>
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

        /// <summary>
        /// Gets the sample rate (frequency) of the active clip in Hz.
        /// </summary>
        /// <returns>
        /// The frequency in Hz, or <c>0</c> if no clip is assigned.
        /// </returns>
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

        /// <summary>
        /// Gets the uncompressed size of the active clip's audio data in bytes.
        /// </summary>
        /// <returns>
        /// The sound size in bytes, or <c>0</c> if no clip is assigned.
        /// </returns>
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

        /// <summary>
        /// Gets the runtime audio compression format of the active clip.
        /// </summary>
        /// <returns>
        /// The <see cref="AudioCompressionFormat"/> of the clip, or
        /// <see cref="AudioCompressionFormat.Vorbis"/> if no clip is assigned.
        /// </returns>
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

        /// <summary>
        /// Gets the audio compression format for the active clip on the current build target platform.
        /// </summary>
        /// <returns>
        /// The target-platform <see cref="AudioCompressionFormat"/>, or
        /// <see cref="AudioCompressionFormat.Vorbis"/> if no clip is assigned.
        /// </returns>
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

        /// <summary>
        /// Gets the duration of the active clip in seconds.
        /// </summary>
        /// <returns>
        /// The duration in seconds as a <see cref="float"/>, or <c>0</c> if no clip is assigned.
        /// </returns>
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

        /// <summary>
        /// Stops playback and releases resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Stops all currently playing audio preview clips in the editor, regardless of which
        /// <see cref="EditorAudioSource"/> instance started them.
        /// </summary>
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
            /// <summary>
            /// Initializes a new <see cref="MockAudioSource"/> with the given clip set as active.
            /// </summary>
            /// <param name="clip">The <see cref="AudioClip"/> to assign on construction.</param>
            public MockAudioSource(AudioClip clip)
            {
                SetClip(clip);
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.StopMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for stopping preview clips.</returns>
            public MethodInfo GetStopMethod()
            {
                return StopMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.PlayMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for playing preview clips.</returns>
            public MethodInfo GetPlayMethod()
            {
                return PlayMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.IsPlayingMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for checking preview clip playback state.</returns>
            public MethodInfo GetIsPlayingMethod()
            {
                return IsPlayingMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.GetDurationMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for retrieving clip duration.</returns>
            public MethodInfo GetGetDurationMethod()
            {
                return GetDurationMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.GetTargetPlatformSoundCompressionFormatMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for retrieving the target platform compression format.</returns>
            public MethodInfo GetGetTargetPlatformSoundCompressionFormatMethod()
            {
                return GetTargetPlatformSoundCompressionFormatMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.GetSoundCompressionFormatMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for retrieving the clip's compression format.</returns>
            public MethodInfo GetGetSoundCompressionFormatMethod()
            {
                return GetSoundCompressionFormatMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.GetSoundSizeMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for retrieving the clip's uncompressed sound size.</returns>
            public MethodInfo GetGetSoundSizeMethod()
            {
                return GetSoundSizeMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.GetFrequencyMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for retrieving the clip's sample rate.</returns>
            public MethodInfo GetGetFrequencyMethod()
            {
                return GetFrequencyMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.GetBitsPerSampleMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for retrieving the clip's bit depth.</returns>
            public MethodInfo GetGetBitsPerSampleMethod()
            {
                return GetBitsPerSampleMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.GetBitRateMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for retrieving the clip's bit rate.</returns>
            public MethodInfo GetGetBitRateMethod()
            {
                return GetBitRateMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.GetChannelCountMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for retrieving the clip's channel count.</returns>
            public MethodInfo GetGetChannelCountMethod()
            {
                return GetChannelCountMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.GetSampleCountMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for retrieving the clip's total sample count.</returns>
            public MethodInfo GetGetSampleCountMethod()
            {
                return GetSampleCountMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.GetClipSamplePositionMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for retrieving the current playback sample position.</returns>
            public MethodInfo GetGetClipSamplePositionMethod()
            {
                return GetClipSamplePositionMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.GetClipPositionMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for retrieving the current playback position in seconds.</returns>
            public MethodInfo GetGetClipPositionMethod()
            {
                return GetClipPositionMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.StopAllClipsMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for stopping all preview clips.</returns>
            public MethodInfo GetStopAllClipsMethod()
            {
                return StopAllClipsMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.LoopClipMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for toggling clip looping.</returns>
            public MethodInfo GetLoopClipMethod()
            {
                return LoopClipMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.ResumeClipMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for resuming a paused clip.</returns>
            public MethodInfo GetResumeClipMethod()
            {
                return ResumeClipMethod;
            }

            /// <summary>
            /// Exposes the protected <see cref="EditorAudioSource.PauseClipMethod"/> for test assertions.
            /// </summary>
            /// <returns>The resolved <see cref="MethodInfo"/> for pausing a playing clip.</returns>
            public MethodInfo GetPauseClipMethod()
            {
                return PauseClipMethod;
            }
        }

        /// <summary>
        /// Creates a <see cref="MockAudioSource"/> loaded with the test audio clip before each test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _audioSource = new MockAudioSource(Resources.Load<AudioClip>("EditorAudio/Awesome"));
        }

        /// <summary>
        /// Verifies that constructing an <see cref="EditorAudioSource"/> does not throw any exceptions.
        /// </summary>
        [Test]
        public void WhenConstructed_TheEditorAudioSource_DoesNotThrowExceptions()
        {
            Assert.DoesNotThrow(() => { new MockAudioSource(Resources.Load<AudioClip>("EditorAudio/Awesome")); });
        }

        /// <summary>
        /// Verifies that all reflected <c>AudioUtil</c> methods are successfully resolved after construction.
        /// </summary>
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

        /// <summary>
        /// Verifies that calling <see cref="EditorAudioSource.Play(int, bool)"/> does not throw an exception.
        /// </summary>
        [Test]
        public void Play_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.Play());
        }

        /// <summary>
        /// Verifies that calling <see cref="EditorAudioSource.Stop"/> after playback does not throw an exception.
        /// </summary>
        [Test]
        public void Stop_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() =>
            {
                _audioSource.Play();
                _audioSource.Stop();
            });
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.IsPlaying"/> returns <c>true</c> during active playback
        /// without throwing an exception.
        /// </summary>
        [Test]
        public void IsPlayingMethod_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() =>
            {
                _audioSource.Play();
                Assert.True(_audioSource.IsPlaying);
            });
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.GetDuration"/> does not throw an exception.
        /// </summary>
        [Test]
        public void GetDuration_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetDuration());
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.GetTargetPlatformSoundCompressionFormat"/> does not throw an exception.
        /// </summary>
        [Test]
        public void GetTargetPlatformSoundCompressionFormat_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetTargetPlatformSoundCompressionFormat());
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.GetSoundCompressionFormat"/> does not throw an exception.
        /// </summary>
        [Test]
        public void GetSoundCompressionFormat_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetSoundCompressionFormat());
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.GetSoundSize"/> does not throw an exception.
        /// </summary>
        [Test]
        public void GetSoundSize_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetSoundSize());
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.GetFrequency"/> does not throw an exception.
        /// </summary>
        [Test]
        public void GetFrequency_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetFrequency());
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.GetBitsPerSample"/> does not throw an exception.
        /// </summary>
        [Test]
        public void GetBitsPerSample_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetBitsPerSample());
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.GetBitRate"/> does not throw an exception.
        /// </summary>
        [Test]
        public void GetBitRate_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetBitRate());
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.GetChannelCount"/> does not throw an exception.
        /// </summary>
        [Test]
        public void GetChannelCount_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetChannelCount());
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.GetSampleCount"/> does not throw an exception.
        /// </summary>
        [Test]
        public void GetSampleCount_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetSampleCount());
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.GetClipSamplePosition"/> does not throw an exception.
        /// </summary>
        [Test]
        public void GetClipSamplePosition_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetClipSamplePosition());
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.GetClipPosition"/> does not throw an exception.
        /// </summary>
        [Test]
        public void GetClipPosition_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _audioSource.GetClipPosition());
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.StopAllClips"/> does not throw an exception.
        /// </summary>
        [Test]
        public void StopAllAudioClips_DoesNotThrowException()
        {
            Assert.DoesNotThrow(EditorAudioSource.StopAllClips);
        }

        /// <summary>
        /// Verifies that <see cref="EditorAudioSource.LoopClip"/> enables looping and does not throw an exception.
        /// </summary>
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

        /// <summary>
        /// Verifies that calling <see cref="EditorAudioSource.ResumeClip"/> after pausing does not throw an exception.
        /// </summary>
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

        /// <summary>
        /// Verifies that calling <see cref="EditorAudioSource.PauseClip"/> during playback does not throw an exception.
        /// </summary>
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
