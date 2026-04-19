using System.IO;
using System.Linq;
using System.Text;
using HamerSoft.Victoria.Core.Extractor;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Tests.Editor.Helpers;
using NUnit.Framework;

namespace HamerSoft.Victoria.Tests.Editor
{
    [TestFixture]
    public class ExtractorTests
    {
        private FileInfo _packageFile;

        [TearDown]
        public void TearDown()
        {
            if (_packageFile != null && _packageFile.Exists)
                _packageFile.Delete();
        }

        private static Asset FindAsset(Folder folder, string name)
            => folder.Children.OfType<Asset>().FirstOrDefault(a => a.Name == name);

        [Test]
        public void Parse_RootFolderName_MatchesPackageFilename()
        {
            _packageFile = new UnityPackageBuilder("mypackage.unitypackage")
                .AddAsset("Scripts/MyScript.cs")
                .Build();

            var root = Extractor.Parse(_packageFile);

            Assert.AreEqual("mypackage.unitypackage", root.Name);
        }

        [Test]
        public void Parse_EmptyPackage_ReturnsRootFolderWithNoChildren()
        {
            _packageFile = new UnityPackageBuilder("empty.unitypackage")
                .Build();

            var root = Extractor.Parse(_packageFile);

            Assert.IsFalse(root.HasChildren);
        }

        [Test]
        public void Parse_SingleAsset_ReturnsCorrectName()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/MyScript.cs")
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Scripts", out var scripts);
            var asset = FindAsset(scripts, "MyScript");
            Assert.AreEqual("MyScript", asset.Name);
        }

        [Test]
        public void Parse_SingleAsset_ReturnsCorrectContentType()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/MyScript.cs")
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Scripts", out var scripts);
            var asset = FindAsset(scripts, "MyScript");
            Assert.AreEqual(".cs", asset.ContentType);
        }

        [Test]
        public void Parse_SingleAsset_ReturnsCorrectFileContent()
        {
            var expectedContent = Encoding.UTF8.GetBytes("public class MyScript {}");
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/MyScript.cs", content: expectedContent)
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Scripts", out var scripts);
            var asset = FindAsset(scripts, "MyScript");
            Assert.AreEqual(expectedContent, asset.FileContent);
        }

        [Test]
        public void Parse_SingleAsset_ReturnsCorrectMetaFile()
        {
            const string meta = "guid: abc123\nMonoImporter: {}";
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/MyScript.cs", meta: meta)
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Scripts", out var scripts);
            var asset = FindAsset(scripts, "MyScript");
            Assert.AreEqual(meta, asset.MetaFile);
        }

        [Test]
        public void Parse_AssetWithoutContent_FileContentIsNull()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/MyScript.cs", content: null)
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Scripts", out var scripts);
            var asset = FindAsset(scripts, "MyScript");
            Assert.IsNull(asset.FileContent);
        }

        [Test]
        public void Parse_AssetWithoutMeta_MetaFileIsNull()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/MyScript.cs", meta: null)
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Scripts", out var scripts);
            var asset = FindAsset(scripts, "MyScript");
            Assert.IsNull(asset.MetaFile);
        }

        [Test]
        public void Parse_AssetWithPreview_ReturnsPreviewContent()
        {
            var previewBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG magic bytes
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Textures/MyTexture.png",
                    previewContent: previewBytes,
                    previewExtension: ".png")
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Textures", out var textures);
            var asset = FindAsset(textures, "MyTexture");
            Assert.AreEqual(previewBytes, asset.PreviewContent);
        }

        [Test]
        public void Parse_AssetWithPreview_ReturnsPreviewType()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Textures/MyTexture.png",
                    previewContent: new byte[] { 1, 2, 3 },
                    previewExtension: ".png")
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Textures", out var textures);
            var asset = FindAsset(textures, "MyTexture");
            Assert.AreEqual(".png", asset.PreviewType);
        }

        [Test]
        public void Parse_AssetWithoutPreview_PreviewContentIsNull()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/MyScript.cs")
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Scripts", out var scripts);
            var asset = FindAsset(scripts, "MyScript");
            Assert.IsNull(asset.PreviewContent);
        }

        [Test]
        public void Parse_TopLevelAsset_AddedDirectlyToRoot()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("MyScript.cs")
                .Build();

            var root = Extractor.Parse(_packageFile);

            var asset = FindAsset(root, "MyScript");
            Assert.IsNotNull(asset);
        }

        [Test]
        public void Parse_SingleFolderAsset_CreatesFolderInRoot()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/MyScript.cs")
                .Build();

            var root = Extractor.Parse(_packageFile);

            var found = root.TryGetChild("Scripts", out _);
            Assert.IsTrue(found);
        }

        [Test]
        public void Parse_NestedFolderAsset_CreatesFullFolderHierarchy()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/Utils/Helper.cs")
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Scripts", out var scripts);
            Assert.IsNotNull(scripts, "Expected 'Scripts' folder");

            var found = scripts.TryGetChild("Utils", out var utils);
            Assert.IsTrue(found, "Expected 'Utils' folder inside 'Scripts'");
            Assert.IsNotNull(FindAsset(utils, "Helper"), "Expected 'Helper' asset inside 'Utils'");
        }

        [Test]
        public void Parse_ComDotPrefixedSegment_TreatedAsFolder()
        {
            // "com.example" has an extension-like dot but is treated as a folder, not an asset
            _packageFile = new UnityPackageBuilder()
                .AddAsset("com.example/Scripts/MyScript.cs")
                .Build();

            var root = Extractor.Parse(_packageFile);

            var found = root.TryGetChild("com.example", out var comFolder);
            Assert.IsTrue(found, "Expected 'com.example' to be a folder");
            comFolder.TryGetChild("Scripts", out var scripts);
            Assert.IsNotNull(FindAsset(scripts, "MyScript"));
        }

        [Test]
        public void Parse_TwoAssetsInSameFolder_FolderCreatedOnce()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/ScriptA.cs")
                .AddAsset("Scripts/ScriptB.cs")
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Scripts", out var scripts);
            Assert.AreEqual(1, root.Children.OfType<Folder>().Count(f => f.Name == "Scripts"),
                "Expected exactly one 'Scripts' folder");
            Assert.IsNotNull(FindAsset(scripts, "ScriptA"));
            Assert.IsNotNull(FindAsset(scripts, "ScriptB"));
        }

        [Test]
        public void Parse_MultipleAssets_AllPresent()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/ScriptA.cs")
                .AddAsset("Scripts/ScriptB.cs")
                .AddAsset("Textures/Logo.png")
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Scripts", out var scripts);
            root.TryGetChild("Textures", out var textures);

            Assert.IsNotNull(FindAsset(scripts, "ScriptA"), "ScriptA missing");
            Assert.IsNotNull(FindAsset(scripts, "ScriptB"), "ScriptB missing");
            Assert.IsNotNull(FindAsset(textures, "Logo"), "Logo missing");
        }

        [Test]
        public void Parse_MultipleAssets_NoDuplication()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/ScriptA.cs")
                .AddAsset("Scripts/ScriptB.cs")
                .Build();

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Scripts", out var scripts);
            Assert.AreEqual(2, scripts.Children.Count);
        }

        [Test]
        public void Parse_DoubleZeroBlockTerminator_DoesNotThrow()
        {
            // POSIX tar requires two consecutive zero blocks. The Extractor stops on
            // the first, so the second must be silently ignored rather than misread as a header.
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/MyScript.cs")
                .Build(terminatorBlocks: 2);

            Assert.DoesNotThrow(() => Extractor.Parse(_packageFile));
        }

        [Test]
        public void Parse_DoubleZeroBlockTerminator_LastAssetIsIncluded()
        {
            _packageFile = new UnityPackageBuilder()
                .AddAsset("Scripts/MyScript.cs")
                .Build(terminatorBlocks: 2);

            var root = Extractor.Parse(_packageFile);

            root.TryGetChild("Scripts", out var scripts);
            Assert.IsNotNull(FindAsset(scripts, "MyScript"));
        }

        [Test]
        public void Parse_Asset_PathResolvedIncludingRootFolder()
        {
            _packageFile = new UnityPackageBuilder("mypackage.unitypackage")
                .AddAsset("Scripts/MyScript.cs")
                .Build();

            var root = Extractor.Parse(_packageFile);
            root.TryGetChild("Scripts", out var scripts);
            var asset = FindAsset(scripts, "MyScript");

            var expected = Path.Combine("mypackage.unitypackage", "Scripts", "MyScript");
            Assert.AreEqual(expected, asset.Path);
        }

        [Test]
        public void Parse_Asset_FullPathIsPathPlusContentType()
        {
            _packageFile = new UnityPackageBuilder("mypackage.unitypackage")
                .AddAsset("Scripts/MyScript.cs")
                .Build();

            var root = Extractor.Parse(_packageFile);
            root.TryGetChild("Scripts", out var scripts);
            var asset = FindAsset(scripts, "MyScript");

            var expected = Path.Combine("mypackage.unitypackage", "Scripts", "MyScript") + ".cs";
            Assert.AreEqual(expected, asset.FullPath);
        }
    }
}
