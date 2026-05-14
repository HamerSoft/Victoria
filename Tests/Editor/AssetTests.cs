using HamerSoft.Victoria.Core.Extractor.Nodes;
using NUnit.Framework;

namespace HamerSoft.Victoria.Tests.Editor
{
    [TestFixture]
    public class AssetTests
    {
        [TestCase(".cs")]
        [TestCase(".json")]
        [TestCase(".meta")]
        [TestCase(".md")]
        [TestCase(".txt")]
        [TestCase(".uss")]
        [TestCase(".uxml")]
        [TestCase(".asmdef")]
        [TestCase(".asset")]
        public void GetPreviewType_PlainTextExtension_ReturnsPlainText(string extension)
        {
            var asset = new Asset { ContentType = extension };
            Assert.AreEqual(Asset.Preview.PlainText, asset.GetPreviewType());
        }

        [TestCase(".mp3")]
        [TestCase(".wav")]
        [TestCase(".ogg")]
        public void GetPreviewType_AudioExtension_ReturnsAudio(string extension)
        {
            var asset = new Asset { ContentType = extension };
            Assert.AreEqual(Asset.Preview.Audio, asset.GetPreviewType());
        }

        [TestCase(".png")]
        [TestCase(".jpg")]
        [TestCase(".jpeg")]
        public void GetPreviewType_ImagePreviewType_ReturnsImage(string previewType)
        {
            var asset = new Asset { ContentType = ".fbx", PreviewType = previewType };
            Assert.AreEqual(Asset.Preview.Image, asset.GetPreviewType());
        }

        [TestCase(".PNG")]
        [TestCase(".JPG")]
        [TestCase(".JPEG")]
        public void GetPreviewType_ImagePreviewTypeUpperCase_ReturnsImage(string previewType)
        {
            var asset = new Asset { ContentType = ".fbx", PreviewType = previewType };
            Assert.AreEqual(Asset.Preview.Image, asset.GetPreviewType());
        }

        [Test]
        public void GetPreviewType_UnknownContentTypeAndNullPreviewType_ReturnsNotAvailable()
        {
            var asset = new Asset { ContentType = ".xyz", PreviewType = null };
            Assert.AreEqual(Asset.Preview.NotAvailable, asset.GetPreviewType());
        }

        [Test]
        public void GetPreviewType_UnknownContentTypeAndUnknownPreviewType_ReturnsNotAvailable()
        {
            var asset = new Asset { ContentType = ".xyz", PreviewType = ".tiff" };
            Assert.AreEqual(Asset.Preview.NotAvailable, asset.GetPreviewType());
        }

        [Test]
        public void GetPreviewType_NullContentTypeAndNullPreviewType_ReturnsNotAvailable()
        {
            var asset = new Asset { ContentType = null, PreviewType = null };
            Assert.AreEqual(Asset.Preview.NotAvailable, asset.GetPreviewType());
        }

        [Test]
        public void GetPreviewType_PlainTextContentTypeWithImagePreviewType_ReturnsPlainText()
        {
            // ContentType match wins — PreviewType is irrelevant for text/audio types
            var asset = new Asset { ContentType = ".cs", PreviewType = ".png" };
            Assert.AreEqual(Asset.Preview.PlainText, asset.GetPreviewType());
        }

        [Test]
        public void GetPreviewType_AudioContentTypeWithModelPreviewType_ReturnsAudio()
        {
            var asset = new Asset { ContentType = ".mp3", PreviewType = ".fbx" };
            Assert.AreEqual(Asset.Preview.Audio, asset.GetPreviewType());
        }

        [Test]
        public void Size_ReturnsFileContentLength()
        {
            var asset = new Asset { FileContent = new byte[42] };
            Assert.AreEqual(42, asset.Size);
        }

        [Test]
        public void Size_EmptyFileContent_ReturnsZero()
        {
            var asset = new Asset { FileContent = new byte[0] };
            Assert.AreEqual(0, asset.Size);
        }

        [Test]
        public void IsLeaf_IsAlwaysTrue()
        {
            var asset = new Asset();
            Assert.IsTrue(asset.IsLeaf);
        }

        [Test]
        public void DetailedName_ReturnsConcatenationOfNameAndContentType()
        {
            var asset = new Asset { Name = "MyScript", ContentType = ".cs" };
            Assert.AreEqual("MyScript.cs", asset.DetailedName);
        }

        [Test]
        public void DetailedName_NullContentType_ReturnsNameOnly()
        {
            var asset = new Asset { Name = "MyScript", ContentType = null };
            Assert.AreEqual("MyScript", asset.DetailedName);
        }

        [Test]
        public void ToString_ReturnsName()
        {
            var asset = new Asset { Name = "MyScript" };
            Assert.AreEqual("MyScript", asset.ToString());
        }
    }
}
