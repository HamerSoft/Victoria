using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Audio;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Core.Search;
using HamerSoft.Victoria.Loader.Loader;
using NUnit.Framework;
using UnityEngine;

namespace HamerSoft.Victoria.Tests.Editor
{
    [TestFixture]
    public class UnityPackageTests
    {
        private class FakeObjectLoader : IObjectLoader
        {
            public int CallCount { get; private set; }
            private readonly object _returnValue;

            public FakeObjectLoader(object returnValue = null) => _returnValue = returnValue;

            public Task<T> LoadObject<T>(string id, byte[] data, Asset.Preview type, CancellationToken token)
            {
                CallCount++;
                return Task.FromResult((T)_returnValue);
            }
        }

        private class NullSearch : ISearch
        {
            public IEnumerable<Node> SearchByName(string currentSearchTerm) => System.Array.Empty<Node>();
        }

        private class NullAudioSource : IAudioSource
        {
            public bool IsPlaying => false;
            public void Play(AudioClip clip) { }
            public void Stop() { }
            public void Dispose() { }
        }

        private static UnityPackage MakePackage(IObjectLoader loader, string rootName = "Root") =>
            new UnityPackage(new Folder(rootName), loader, new NullSearch(), new NullAudioSource());

        private static readonly byte[] SomeData = { 1, 2, 3 };

        [Test]
        public async Task LoadObject_FirstCall_InvokesLoaderOnce()
        {
            var loader = new FakeObjectLoader("result");
            var package = MakePackage(loader);

            await package.LoadObject<string>("id", SomeData, Asset.Preview.PlainText, CancellationToken.None);

            Assert.AreEqual(1, loader.CallCount);
        }

        [Test]
        public async Task LoadObject_SecondCallSameIdAndType_ReturnsCachedValue()
        {
            var loader = new FakeObjectLoader("cached");
            var package = MakePackage(loader);

            await package.LoadObject<string>("id", SomeData, Asset.Preview.PlainText, CancellationToken.None);
            var result = await package.LoadObject<string>("id", SomeData, Asset.Preview.PlainText, CancellationToken.None);

            Assert.AreEqual(1, loader.CallCount);
            Assert.AreEqual("cached", result);
        }

        [Test]
        public async Task LoadObject_NullResult_IsNotCached()
        {
            var loader = new FakeObjectLoader(null);
            var package = MakePackage(loader);

            await package.LoadObject<string>("id", SomeData, Asset.Preview.PlainText, CancellationToken.None);
            await package.LoadObject<string>("id", SomeData, Asset.Preview.PlainText, CancellationToken.None);

            Assert.AreEqual(2, loader.CallCount);
        }

        [Test]
        public async Task LoadObject_DifferentTypesSameId_CacheKeysAreIndependent()
        {
            var loader = new FakeObjectLoader("value");
            var package = MakePackage(loader);

            await package.LoadObject<string>("x", SomeData, Asset.Preview.PlainText, CancellationToken.None);
            await package.LoadObject<string>("x", SomeData, Asset.Preview.Image, CancellationToken.None);

            Assert.AreEqual(2, loader.CallCount);
        }

        [Test]
        public async Task LoadObject_AfterDispose_CacheIsCleared()
        {
            var loader = new FakeObjectLoader("value");
            var package = MakePackage(loader);

            await package.LoadObject<string>("id", SomeData, Asset.Preview.PlainText, CancellationToken.None);
            package.Dispose();
            await package.LoadObject<string>("id", SomeData, Asset.Preview.PlainText, CancellationToken.None);

            Assert.AreEqual(2, loader.CallCount);
        }

        [Test]
        public void Name_ReturnsRootFolderName()
        {
            var package = MakePackage(new FakeObjectLoader(), rootName: "MyPackage.unitypackage");

            Assert.AreEqual("MyPackage.unitypackage", package.Name);
        }
    }
}
