using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Loader;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HamerSoft.Victoria.Tests.Editor
{
    [TestFixture]
    public class ObjectLoaderTests
    {
        private ObjectLoader _loader;
        private Texture2D _texture;

        [SetUp]
        public void SetUp()
        {
            _loader = new ObjectLoader();
        }

        [TearDown]
        public void TearDown()
        {
            if (_texture != null)
            {
                Object.DestroyImmediate(_texture);
                _texture = null;
            }
        }
        
        [Test]
        public async Task LoadObject_EmptyData_ThrowsArgumentException()
        {
            Exception caught = null;
            try
            {
                await _loader.LoadObject<string>("id", new byte[0], Asset.Preview.PlainText,
                    CancellationToken.None);
            }
            catch (ArgumentException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught, "Expected ArgumentException for empty data.");
        }

        [TestCase(Asset.Preview.Image)]
        [TestCase(Asset.Preview.Audio)]
        public async Task LoadObject_EmptyData_AnyPreviewType_ThrowsArgumentException(Asset.Preview type)
        {
            Exception caught = null;
            try
            {
                await _loader.LoadObject<object>("id", new byte[0], type, CancellationToken.None);
            }
            catch (ArgumentException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught, $"Expected ArgumentException for empty data with type {type}.");
        }
        
        [Test]
        public async Task LoadObject_PlainText_AsciiBytes_ReturnsDecodedString()
        {
            var expected = "Hello, Victoria!";
            var data = Encoding.UTF8.GetBytes(expected);

            var result = await _loader.LoadObject<string>("id", data, Asset.Preview.PlainText,
                CancellationToken.None);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task LoadObject_PlainText_MultiByteUtf8_ReturnsDecodedString()
        {
            var expected = "Héllo Wörld — 日本語";
            var data = Encoding.UTF8.GetBytes(expected);

            var result = await _loader.LoadObject<string>("id", data, Asset.Preview.PlainText,
                CancellationToken.None);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task LoadObject_PlainText_SingleByte_ReturnsOneCharString()
        {
            var data = Encoding.UTF8.GetBytes("A");

            var result = await _loader.LoadObject<string>("id", data, Asset.Preview.PlainText,
                CancellationToken.None);

            Assert.AreEqual("A", result);
        }

        [Test]
        public async Task LoadObject_PlainText_CancelledToken_ThrowsOperationCanceledException()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var data = Encoding.UTF8.GetBytes("some content");

            Exception caught = null;
            try
            {
                await _loader.LoadObject<string>("id", data, Asset.Preview.PlainText, cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught, "Expected OperationCanceledException when token is pre-cancelled.");
        }
        
        [TestCase(Asset.Preview.NotAvailable)]
        [TestCase(Asset.Preview.NotSupported)]
        public async Task LoadObject_UnsupportedPreviewType_ThrowsArgumentOutOfRangeException(Asset.Preview type)
        {
            var data = Encoding.UTF8.GetBytes("irrelevant");

            Exception caught = null;
            try
            {
                await _loader.LoadObject<object>("id", data, type, CancellationToken.None);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught, $"Expected ArgumentOutOfRangeException for type {type}.");
        }
        
        [Test]
        public async Task LoadObject_Image_ValidPng_ReturnsNonNullTexture2D()
        {
            var pngBytes = CreateMinimalPng();

            _texture = await _loader.LoadObject<Texture2D>("img-id", pngBytes, Asset.Preview.Image,
                CancellationToken.None);

            Assert.IsNotNull(_texture);
        }

        [Test]
        public async Task LoadObject_Image_ValidPng_ReturnsDimensionsMatchingSource()
        {
            var source = new Texture2D(4, 4);
            source.SetPixels(new Color[16]);
            source.Apply();
            var pngBytes = source.EncodeToPNG();
            Object.DestroyImmediate(source);

            _texture = await _loader.LoadObject<Texture2D>("img-id", pngBytes, Asset.Preview.Image,
                CancellationToken.None);

            Assert.AreEqual(4, _texture.width);
            Assert.AreEqual(4, _texture.height);
        }

        [Test]
        public async Task LoadObject_Image_InvalidData_ReturnsNull()
        {
            var garbage = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x01, 0x02, 0x03 };

            _texture = await _loader.LoadObject<Texture2D>("img-id", garbage, Asset.Preview.Image,
                CancellationToken.None);

            Assert.IsNull(_texture);
        }
        
        /// <summary>
        /// Creates a minimal 1x1 white PNG using Unity's encoder so the test has
        /// no hardcoded binary blobs and stays readable as the platform evolves.
        /// </summary>
        private static byte[] CreateMinimalPng()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            var png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            return png;
        }
    }
}
