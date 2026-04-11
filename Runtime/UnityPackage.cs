using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Audio;
using HamerSoft.Victoria.Core.Extractor;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Core.Import;
using HamerSoft.Victoria.Core.Search;
using HamerSoft.Victoria.Loader;
using HamerSoft.Victoria.Loader.Loader;
using HamerSoft.Victoria.Ui;
using UnityEngine;

namespace HamerSoft.Victoria
{
    public class UnityPackage : IDisposable
    {
        private const int DEFAULT_CAPACITY = 50;
        private readonly IObjectLoader _loader;
        private readonly Dictionary<string, object> _cache;
        public readonly ISearch Search;
        public readonly string Name;
        public Folder Assets { get; }
        public IAudioSource AudioSource { get; }

        internal UnityPackage(Folder assets, IObjectLoader loader, ISearch search, IAudioSource audioSource)
        {
            Name = assets.Name;
            AudioSource = audioSource;
            Search = search;
            Assets = assets;
            _loader = loader;
            _cache = new Dictionary<string, object>(DEFAULT_CAPACITY);
        }

        public async Task<T> LoadObject<T>(string id, byte[] data, Asset.Preview type,
            CancellationToken token)
        {
            id = GenerateId(id, type);
            if (TryGetFromCache<T>(id, out var loadedObject))
                return loadedObject;

            loadedObject = await _loader.LoadObject<T>(id, data, type, token);
            if (loadedObject != null)
                AddToCache(id, loadedObject);
            return loadedObject;
        }

        private bool TryGetFromCache<T>(string id, out T cachedObject)
        {
            cachedObject = default;
            if (_cache.TryGetValue(id, out var cached))
                cachedObject = (T)(object)cached;

            return cachedObject != null;
        }

        private void AddToCache(string id, object cacheObject)
        {
            _cache.Add(id, cacheObject);
        }

        private string GenerateId(string id, Asset.Preview type)
        {
            return $"{type.ToString()}-{id}";
        }

        public void Dispose()
        {
            foreach (var item in _cache)
                if (item.Value is UnityEngine.Object unityObject)
                    UnityEngine.Object.Destroy(unityObject);
            _cache.Clear();
            AudioSource?.Dispose();
        }

        internal async Task Import(string rootImportPath, ImportManifest importManifest, Action<string> onUpdate)
        {
            using var importer = new Importer(rootImportPath, importManifest);
            await importer.Import(onUpdate);
        }

        public static UnityPackage LoadFromPath(FileInfo selectedPackage, IAudioSource audioSource)
        {
            try
            {
                var assets = Extractor.Parse(selectedPackage);
                if (assets != null)
                {
                    return new UnityPackage(assets, new ObjectLoader(), new Searcher(assets),
                        audioSource);
                }

                throw new FileLoadException("Failed to load package " + selectedPackage);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load package {e}");
                throw;
            }
        }
    }
}