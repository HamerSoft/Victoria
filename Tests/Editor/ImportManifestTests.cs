using System.Collections.Generic;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Core.Import;
using HamerSoft.Victoria.Ui.Elements.Nodes;
using NUnit.Framework;

namespace HamerSoft.Victoria.Tests.Editor
{
    [TestFixture]
    public class ImportManifestTests
    {
        private static Asset MakeAsset(string name = "MyAsset") =>
            new Asset { Name = name, ContentType = ".cs", FileContent = new byte[0] };

        private static SelectableNode MakeSelectableNode(Node node) =>
            new SelectableNode(node, 0, (SelectableNode)null);

        [Test]
        public void Contains_AfterAdd_ReturnsTrue()
        {
            var manifest = new ImportManifest();
            var asset = MakeAsset();
            var uiNode = MakeSelectableNode(asset);
            var parentUiNode = MakeSelectableNode(new Folder("Root"));

            manifest.Add(uiNode, parentUiNode);

            Assert.IsTrue(manifest.Contains(asset));
        }

        [Test]
        public void Contains_NodeNeverAdded_ReturnsFalse()
        {
            var manifest = new ImportManifest();
            var asset = MakeAsset();

            Assert.IsFalse(manifest.Contains(asset));
        }

        [Test]
        public void Contains_AfterRemove_ReturnsFalse()
        {
            var manifest = new ImportManifest();
            var asset = MakeAsset();
            var uiNode = MakeSelectableNode(asset);

            manifest.Add(uiNode, null);
            manifest.Remove(uiNode);

            Assert.IsFalse(manifest.Contains(asset));
        }

        [Test]
        public void Remove_DecreasesImportsCount()
        {
            var manifest = new ImportManifest();
            var asset = MakeAsset();
            var uiNode = MakeSelectableNode(asset);

            manifest.Add(uiNode, null);
            manifest.Remove(uiNode);

            Assert.AreEqual(0, manifest.Imports.Count);
        }

        [Test]
        public void Add_SameSelectableNodeTwice_UpdatesParentEntry()
        {
            var manifest = new ImportManifest();
            var asset = MakeAsset();
            var uiNode = MakeSelectableNode(asset);
            var parentA = MakeSelectableNode(new Folder("ParentA"));
            var parentB = MakeSelectableNode(new Folder("ParentB"));

            manifest.Add(uiNode, parentA);
            manifest.Add(uiNode, parentB);

            Assert.AreEqual(1, manifest.Imports.Count);
            Assert.AreSame(parentB, manifest.Imports[uiNode]);
        }

        [Test]
        public void Imports_IsIReadOnlyDictionary()
        {
            var manifest = new ImportManifest();
            Assert.IsInstanceOf<IReadOnlyDictionary<SelectableNode, BaseUiNode>>(manifest.Imports);
        }

        [Test]
        public void Imports_ReflectsCurrentState()
        {
            var manifest = new ImportManifest();
            var asset = MakeAsset();
            var uiNode = MakeSelectableNode(asset);

            Assert.AreEqual(0, manifest.Imports.Count);
            manifest.Add(uiNode, null);
            Assert.AreEqual(1, manifest.Imports.Count);
        }

        [Test]
        public void Destroy_ClearsImports()
        {
            var manifest = new ImportManifest();
            manifest.Add(MakeSelectableNode(MakeAsset("A")), null);
            manifest.Add(MakeSelectableNode(MakeAsset("B")), null);

            manifest.Destroy();

            Assert.AreEqual(0, manifest.Imports.Count);
        }

        [Test]
        public void Destroy_ContainsReturnsFalseForPreviouslyAddedNode()
        {
            var manifest = new ImportManifest();
            var asset = MakeAsset();
            var uiNode = MakeSelectableNode(asset);

            manifest.Add(uiNode, null);
            manifest.Destroy();

            Assert.IsFalse(manifest.Contains(asset));
        }

        [Test]
        public void Contains_OnEmptyManifest_DoesNotThrow()
        {
            var manifest = new ImportManifest();
            Assert.DoesNotThrow(() => manifest.Contains(MakeAsset()));
        }

        [Test]
        public void Remove_OnEmptyManifest_DoesNotThrow()
        {
            var manifest = new ImportManifest();
            var uiNode = MakeSelectableNode(MakeAsset());
            Assert.DoesNotThrow(() => manifest.Remove(uiNode));
        }

        [Test]
        public void Destroy_OnEmptyManifest_DoesNotThrow()
        {
            var manifest = new ImportManifest();
            Assert.DoesNotThrow(() => manifest.Destroy());
        }
    }
}
