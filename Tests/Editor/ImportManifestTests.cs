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
        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static Asset MakeAsset(string name = "MyAsset") =>
            new Asset { Name = name, ContentType = ".cs", FileContent = new byte[0] };

        /// <summary>
        /// Constructs a SelectableNode without a parent in Edit Mode.
        /// No panel is required — construction only registers callbacks and
        /// sets initial state; no layout or event dispatch occurs.
        /// </summary>
        private static SelectableNode MakeSelectableNode(Node node) =>
            new SelectableNode(node, 0, (SelectableNode)null);

        // -----------------------------------------------------------------------
        // 6.1 — Add then Contains
        // -----------------------------------------------------------------------

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

        // -----------------------------------------------------------------------
        // 6.2 — Contains — absent node
        // -----------------------------------------------------------------------

        [Test]
        public void Contains_NodeNeverAdded_ReturnsFalse()
        {
            var manifest = new ImportManifest();
            var asset = MakeAsset();

            Assert.IsFalse(manifest.Contains(asset));
        }

        // -----------------------------------------------------------------------
        // 6.3 — Remove
        // -----------------------------------------------------------------------

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

        // -----------------------------------------------------------------------
        // 6.4 — Overwrite via Add (same SelectableNode, different parent)
        // -----------------------------------------------------------------------

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

        // -----------------------------------------------------------------------
        // 6.5 — Imports is IReadOnlyDictionary
        // -----------------------------------------------------------------------

        [Test]
        public void Imports_IsIReadOnlyDictionary()
        {
            // The property's declared type is IReadOnlyDictionary<SelectableNode, BaseUiNode>.
            // This prevents callers from calling Add/Remove on the returned reference
            // at compile time. The assertion below verifies the runtime value is
            // compatible with that interface.
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

        // -----------------------------------------------------------------------
        // 6.6 — Destroy
        // -----------------------------------------------------------------------

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

        // -----------------------------------------------------------------------
        // 6.7 — Operations on empty manifest do not throw
        // -----------------------------------------------------------------------

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
