using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Audio;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Core.Search;
using HamerSoft.Victoria.Loader.Loader;
using HamerSoft.Victoria.Ui;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace HamerSoft.Victoria.Tests.Runtime
{
    [TestFixture]
    public class VictoriaRuntimeImporterTests
    {
        private GameObject _uiRoot;
        private UIDocument _uiDoc;
        private FileInfo _packageFile;
        private string _destDir;
        private ThemeStyleSheet _theme;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.themeStyleSheet = null;
            panelSettings.targetTexture = RenderTexture.GetTemporary(1, 1);
            var eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            _uiRoot = new GameObject("TestUI");
            _uiDoc = _uiRoot.AddComponent<UIDocument>();
            _uiDoc.panelSettings = panelSettings;
            _packageFile = MinimalPackageFile.Build();
            _destDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "victoria_dest");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            UnityEngine.Object.Destroy(_uiRoot);
            if (File.Exists(_packageFile.FullName))
                File.Delete(_packageFile.FullName);
            var parentDir = Path.GetDirectoryName(_destDir);
            if (parentDir != null && Directory.Exists(parentDir))
                Directory.Delete(parentDir, true);
            Object.Destroy(_theme);
            Object.Destroy(EventSystem.current.gameObject);

            yield return null;
        }

        private VisualElement Root => _uiDoc.rootVisualElement;

        [Test]
        public void Create_ReturnsImporterAddedToParent()
        {
            Directory.CreateDirectory(_destDir);
            var importer = Ui.Victoria.Create(Root, _packageFile.FullName, _destDir);

            Assert.IsNotNull(importer);
            Assert.AreEqual(Root, importer.parent);

            importer.Dispose();
        }

        [Test]
        public void Create_WithNonExistentDestination_CreatesDirectory()
        {
            Assert.IsFalse(Directory.Exists(_destDir));

            var importer = Ui.Victoria.Create(Root, _packageFile.FullName, _destDir);

            Assert.IsTrue(Directory.Exists(_destDir));

            importer.Dispose();
        }

        [Test]
        public void Close_RemovesImporterFromParentAndDisposesPackage()
        {
            var audioSpy = new SpyAudioSource();
            var folder = new Folder("TestPackage");
            folder.AddChild(new Asset { Name = "Foo", ContentType = ".cs", FileContent = Array.Empty<byte>() });
            var package = new UnityPackage(folder, new NullObjectLoader(), new NullSearch(), audioSpy);

            Directory.CreateDirectory(_destDir);
            var importer = new Ui.Victoria.VictoriaRuntimeImporter(
                package, Root, new DirectoryInfo(_destDir));

            importer.Close();

            Assert.AreEqual(0, Root.childCount);
            Assert.IsTrue(audioSpy.WasDisposed);
        }

        [UnityTest]
        public IEnumerator NavigationMoveEvent_OnImporter_IsStopped()
        {
            Directory.CreateDirectory(_destDir);
            var importer = Ui.Victoria.Create(Root, _packageFile.FullName, _destDir);
            yield return null;

            using var evt = NavigationMoveEvent.GetPooled(NavigationMoveEvent.Direction.Down);
            evt.target = importer;
            importer.SendEvent(evt);

            Assert.IsTrue(evt.isPropagationStopped);

            importer.Dispose();
        }

        [Test]
        public void NavigationMoveEvent_FromOutsideImporter_IsNotStopped()
        {
            Directory.CreateDirectory(_destDir);
            var importer = Ui.Victoria.Create(Root, _packageFile.FullName, _destDir);

            var sibling = new VisualElement();
            Root.Add(sibling);

            using var evt = NavigationMoveEvent.GetPooled(NavigationMoveEvent.Direction.Down);
            evt.target = sibling;
            sibling.SendEvent(evt);

            Assert.IsFalse(evt.isPropagationStopped);

            importer.Close();
        }

        private class SpyAudioSource : IAudioSource
        {
            public bool IsPlaying => false;
            public bool WasDisposed { get; private set; }

            public void Play(AudioClip clip)
            {
            }

            public void Stop()
            {
            }

            public void Dispose() => WasDisposed = true;
        }

        private class NullObjectLoader : IObjectLoader
        {
            public Task<T> LoadObject<T>(string id, byte[] data, Asset.Preview type, CancellationToken token)
                => Task.FromResult(default(T));
        }

        private class NullSearch : ISearch
        {
            public IEnumerable<Node> SearchByName(string currentSearchTerm) => Array.Empty<Node>();
        }

        private static class MinimalPackageFile
        {
            public static FileInfo Build()
            {
                var path = Path.Combine(Path.GetTempPath(), "victoria_runtime_test.unitypackage");
                using var file = File.Create(path);
                using var gzip = new GZipStream(file, CompressionMode.Compress);

                var guid = Guid.NewGuid().ToString("N");
                WriteHeader(gzip, guid + "/", 0, '5');
                var pathBytes = Encoding.UTF8.GetBytes("Assets/Foo.cs\n");
                WriteHeader(gzip, guid + "/pathname", pathBytes.Length, '0');
                WriteData(gzip, pathBytes);
                WriteHeader(gzip, guid + "/asset", 3, '0');
                WriteData(gzip, new byte[] { 1, 2, 3 });
                gzip.Write(new byte[512], 0, 512);

                return new FileInfo(path);
            }

            private static void WriteHeader(Stream s, string name, long size, char type)
            {
                var h = new byte[512];
                var n = Encoding.ASCII.GetBytes(name);
                Array.Copy(n, 0, h, 0, Math.Min(n.Length, 100));
                var sz = Encoding.ASCII.GetBytes(Convert.ToString(size, 8).PadLeft(11, '0'));
                Array.Copy(sz, 0, h, 124, sz.Length);
                h[156] = (byte)type;
                s.Write(h, 0, 512);
            }

            private static void WriteData(Stream s, byte[] data)
            {
                s.Write(data, 0, data.Length);
                var pad = (512 - data.Length % 512) % 512;
                if (pad > 0) s.Write(new byte[pad], 0, pad);
            }
        }
    }
}