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
using UnityEngine;

namespace HamerSoft.Victoria
{
    /// <summary>
    /// Represents a parsed <c>.unitypackage</c> file. Exposes the package's asset tree and provides
    /// methods for loading individual assets on demand and importing them to disk.
    /// Dispose when done to release all cached Unity objects and the audio source.
    /// </summary>
    public class UnityPackage : IDisposable
    {
        private const int DEFAULT_CAPACITY = 50;
        private readonly IObjectLoader _loader;
        private readonly Dictionary<string, object> _cache;

        internal readonly ISearch Search;
        internal IAudioSource AudioSource { get; }
        /// <summary>
        /// The root folder name of the package, derived from the top-level <see cref="Folder"/> in the asset tree.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The root <see cref="Folder"/> of the package's asset tree, containing the full hierarchy
        /// of folders and assets parsed from the <c>.unitypackage</c> file.
        /// </summary>
        public Folder Assets { get; }

        internal UnityPackage(Folder assets, IObjectLoader loader, ISearch search, IAudioSource audioSource)
        {
            Name = assets.Name;
            AudioSource = audioSource;
            Search = search;
            Assets = assets;
            _loader = loader;
            _cache = new Dictionary<string, object>(DEFAULT_CAPACITY);
        }

        /// <summary>
        /// Asynchronously loads a Unity object of type <typeparamref name="T"/> from raw asset data.
        /// The result is cached by <paramref name="id"/> and <paramref name="type"/>; subsequent calls
        /// with the same key return the cached instance without re-loading.
        /// </summary>
        /// <typeparam name="T">The expected Unity object type (e.g. <c>Texture2D</c>, <c>AudioClip</c>).</typeparam>
        /// <param name="id">A unique identifier for the asset within this package.</param>
        /// <param name="data">The raw byte data of the asset to load.</param>
        /// <param name="type">The preview type hint used to determine how to decode <paramref name="data"/>.</param>
        /// <param name="token">Cancellation token to abort the load operation.</param>
        /// <returns>
        /// The loaded object of type <typeparamref name="T"/>, or <c>null</c> if loading failed.
        /// </returns>
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

        /// <summary>
        /// Destroys all cached Unity objects — using <c>DestroyImmediate</c> in the Editor and
        /// <c>Destroy</c> at runtime — clears the cache, and disposes the audio source.
        /// </summary>
        public void Dispose()
        {
            foreach (var item in _cache)
                if (item.Value is UnityEngine.Object unityObject)
                {
                    if (Application.isEditor && Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(unityObject);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(unityObject);
                    }
                }

            _cache.Clear();
            AudioSource?.Dispose();
        }

        internal async Task Import(string rootImportPath, ImportManifest importManifest, Action<string> onUpdate)
        {
            using var importer = new Importer(rootImportPath, importManifest);
            await importer.Import(onUpdate);
        }

        /// <summary>
        /// Parses a <c>.unitypackage</c> file from disk and returns a ready-to-use <see cref="UnityPackage"/>.
        /// </summary>
        /// <param name="selectedPackage">A <see cref="FileInfo"/> pointing to the <c>.unitypackage</c> file.</param>
        /// <param name="audioSource">The audio source used for previewing audio assets in the importer UI.</param>
        /// <returns>A fully parsed <see cref="UnityPackage"/> with its asset tree populated.</returns>
        /// <exception cref="FileLoadException">Thrown if the package cannot be parsed.</exception>
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