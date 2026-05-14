using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Loader.Loader;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace HamerSoft.Victoria.Loader
{
    internal class ObjectLoader : IObjectLoader
    {
        private readonly string _audioCachePath;

        public ObjectLoader()
        {
            _audioCachePath = Path.Combine(Application.temporaryCachePath, "WIDWIW", "ImportCache");
            if (!Directory.Exists(_audioCachePath))
                Directory.CreateDirectory(_audioCachePath);
        }

        public async Task<T> LoadObject<T>(string id, byte[] data, Asset.Preview type,
            CancellationToken token)
        {
            if (data is { Length: 0 })
                return await Task.FromException<T>(new ArgumentException("Cannot load object, data was invalid."));

            return type switch
            {
                Asset.Preview.PlainText => (T)(object)await LoadPlainText(data, token),
                Asset.Preview.Image => (T)(object)LoadTexture(data),
                Asset.Preview.Audio => (T)(object)await LoadAudio(id, data, token),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        // write out to disk, and then load it with www request to avoid codec nonsense.
        private async Task<AudioClip> LoadAudio(string id, byte[] data, CancellationToken token)
        {
            try
            {
                var fileInfo = new FileInfo(Path.Combine("file://", _audioCachePath, id));
                if (!fileInfo.Exists)
                {
                    await File.WriteAllBytesAsync(fileInfo.FullName, data, token);
                }

                await Task.Delay(100, token);
                return await LoadAudioFromDisk(fileInfo.FullName, token);
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        private async Task<AudioClip> LoadAudioFromDisk(string path, CancellationToken token)
        {
            path = "file://" + path.Replace("\\", "/");
            using var request = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN);
            request.SendWebRequest();

            while (!token.IsCancellationRequested &&
                   !request.isDone &&
                   string.IsNullOrWhiteSpace(request.error))
            {
                // ReSharper disable once MethodSupportsCancellation
                await Task.Delay(50);
            }

            if (token.IsCancellationRequested)
                return null;

            if (string.IsNullOrWhiteSpace(request.error))
                return DownloadHandlerAudioClip.GetContent(request);

            Debug.LogWarning($"Failed to load preview audio: {request.error}");
            return null;
        }

        private static Task<string> LoadPlainText(byte[] data, CancellationToken token)
        {
            try
            {
                return Task.Run(() => Encoding.UTF8.GetString(data), token);
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        private static Texture2D LoadTexture(byte[] data)
        {
            var texture = new Texture2D(1, 1);
            if (texture.LoadImage(data))
            {
                return texture;
            }

            if (Application.isEditor && Application.isPlaying)
            {
                Object.Destroy(texture);
            }
            else
            {
                Object.DestroyImmediate(texture);
            }

            return null;
        }
    }
}