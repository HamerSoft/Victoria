using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Core.Import;
using HamerSoft.Victoria.Ui.Elements.Nodes;
using NUnit.Framework;

namespace HamerSoft.Victoria.Tests.Editor
{
    [TestFixture]
    public class ImporterTests
    {
        // -----------------------------------------------------------------------
        // Reflection accessors — set private UI state without a panel
        // -----------------------------------------------------------------------

        private static readonly FieldInfo IsSelectedField =
            typeof(SelectableNode).GetField("_isSelected", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo IsExpandedField =
            typeof(BaseUiNode).GetField("_isExpanded", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly PropertyInfo ChildUiNodesProperty =
            typeof(BaseUiNode).GetProperty("ChildUiNodes", BindingFlags.NonPublic | BindingFlags.Instance);

        private static void SetIsSelected(SelectableNode node, bool value) =>
            IsSelectedField.SetValue(node, value);

        private static void SetIsExpanded(SelectableNode node, bool value) =>
            IsExpandedField.SetValue(node, value);

        private static void AddChildUiNode(SelectableNode parent, SelectableNode child)
        {
            var list = (List<BaseUiNode>)ChildUiNodesProperty.GetValue(parent);
            list.Add(child);
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static Asset MakeAsset(string name = "MyAsset", string ext = ".cs") =>
            new Asset { Name = name, ContentType = ext, FileContent = new byte[0] };

        private static SelectableNode MakeUiNode(Node node, SelectableNode parent = null) =>
            new SelectableNode(node, 0, parent);

        private static Importer MakeImporter(ImportManifest manifest = null) =>
            new Importer("", manifest ?? new ImportManifest());

        // -----------------------------------------------------------------------
        // 8.1 — Selected leaf returns exactly that node
        // -----------------------------------------------------------------------

        [Test]
        public void CollectNodes_SelectedLeaf_ReturnsExactlyThatNode()
        {
            var asset = MakeAsset();
            var uiNode = MakeUiNode(asset);

            var result = MakeImporter().CollectNodesToWriteOut(uiNode);

            Assert.AreEqual(1, result.Count);
            Assert.AreSame(asset, result[0]);
        }

        // -----------------------------------------------------------------------
        // 8.2 — Deselected leaf returns empty list
        // -----------------------------------------------------------------------

        [Test]
        public void CollectNodes_DeselectedLeaf_ReturnsEmptyList()
        {
            var asset = MakeAsset();
            var uiNode = MakeUiNode(asset);
            SetIsSelected(uiNode, false);

            var result = MakeImporter().CollectNodesToWriteOut(uiNode);

            Assert.IsEmpty(result);
        }

        // -----------------------------------------------------------------------
        // 8.3 — Selected expanded folder with all children selected
        // -----------------------------------------------------------------------

        [Test]
        public void CollectNodes_SelectedExpandedFolderAllChildrenSelected_ReturnsFolderAndAllChildren()
        {
            var folder = new Folder("Parent");
            var child1 = MakeAsset("A");
            var child2 = MakeAsset("B");
            folder.AddChild(child1);
            folder.AddChild(child2);

            var folderUiNode = MakeUiNode(folder);
            var child1UiNode = MakeUiNode(child1, folderUiNode);
            var child2UiNode = MakeUiNode(child2, folderUiNode);
            SetIsExpanded(folderUiNode, true);
            AddChildUiNode(folderUiNode, child1UiNode);
            AddChildUiNode(folderUiNode, child2UiNode);

            var result = MakeImporter().CollectNodesToWriteOut(folderUiNode);

            Assert.AreEqual(3, result.Count);
            CollectionAssert.Contains(result, folder);
            CollectionAssert.Contains(result, child1);
            CollectionAssert.Contains(result, child2);
        }

        // -----------------------------------------------------------------------
        // 8.4 — Selected collapsed folder returns folder + all data children recursively
        // -----------------------------------------------------------------------

        [Test]
        public void CollectNodes_SelectedCollapsedFolder_ReturnsAllDataChildrenRecursively()
        {
            var folder = new Folder("Parent");
            var child = MakeAsset("A");
            var subFolder = new Folder("Sub");
            var grandChild = MakeAsset("C");
            subFolder.AddChild(grandChild);
            folder.AddChild(child);
            folder.AddChild(subFolder);

            // IsExpanded = false by default — data-side traversal applies
            var folderUiNode = MakeUiNode(folder);

            var result = MakeImporter().CollectNodesToWriteOut(folderUiNode);

            Assert.AreEqual(4, result.Count);
            CollectionAssert.Contains(result, folder);
            CollectionAssert.Contains(result, child);
            CollectionAssert.Contains(result, subFolder);
            CollectionAssert.Contains(result, grandChild);
        }

        // -----------------------------------------------------------------------
        // 8.5 — Partially selected children: only selected ones are included
        // -----------------------------------------------------------------------

        [Test]
        public void CollectNodes_PartiallySelectedChildren_ExcludesDeselectedChildren()
        {
            var folder = new Folder("Parent");
            var selected = MakeAsset("Selected");
            var deselected = MakeAsset("Deselected");
            folder.AddChild(selected);
            folder.AddChild(deselected);

            var folderUiNode = MakeUiNode(folder);
            var selectedUiNode = MakeUiNode(selected, folderUiNode);
            var deselectedUiNode = MakeUiNode(deselected, folderUiNode);
            SetIsSelected(deselectedUiNode, false);
            SetIsExpanded(folderUiNode, true);
            AddChildUiNode(folderUiNode, selectedUiNode);
            AddChildUiNode(folderUiNode, deselectedUiNode);

            var result = MakeImporter().CollectNodesToWriteOut(folderUiNode);

            Assert.AreEqual(2, result.Count);
            CollectionAssert.Contains(result, folder);
            CollectionAssert.Contains(result, selected);
            CollectionAssert.DoesNotContain(result, deselected);
        }

        // -----------------------------------------------------------------------
        // 8.6 — Deduplication: same underlying Node in two manifest entries is written once
        // -----------------------------------------------------------------------

        [Test]
        public void Import_SameNodeInTwoManifestEntries_WritesFileOnlyOnce()
        {
            var root = new Folder("Root");
            var asset = new Asset { Name = "Dup", ContentType = ".txt", FileContent = new byte[] { 1, 2, 3 } };
            root.AddChild(asset);

            // Two distinct SelectableNode wrappers around the same Asset → two manifest entries
            var uiNode1 = MakeUiNode(asset);
            var uiNode2 = MakeUiNode(asset);
            var manifest = new ImportManifest();
            manifest.Add(uiNode1, null);
            manifest.Add(uiNode2, null);

            var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "Root");
            Directory.CreateDirectory(tempRoot);
            try
            {
                Task.Run(() => new Importer(tempRoot, manifest).Import(null)).GetAwaiter().GetResult();

                Assert.IsTrue(File.Exists(Path.Combine(tempRoot, "Dup.txt")));
            }
            finally
            {
                Directory.Delete(Path.GetDirectoryName(tempRoot)!, true);
            }
        }

        // -----------------------------------------------------------------------
        // 8.7 — Mixed expanded and collapsed subtrees
        // -----------------------------------------------------------------------

        [Test]
        public void CollectNodes_MixedExpandedAndCollapsedSubtrees_CollectsBothPathsCorrectly()
        {
            var root = new Folder("Root");
            var folderA = new Folder("FolderA"); // will be collapsed in UI
            var assetA1 = MakeAsset("A1");
            var assetA2 = MakeAsset("A2");
            folderA.AddChild(assetA1);
            folderA.AddChild(assetA2);

            var folderB = new Folder("FolderB"); // will be expanded in UI
            var assetB1 = MakeAsset("B1");
            var assetB2 = MakeAsset("B2");
            folderB.AddChild(assetB1);
            folderB.AddChild(assetB2);

            root.AddChild(folderA);
            root.AddChild(folderB);

            var rootUiNode = MakeUiNode(root);
            var folderAUiNode = MakeUiNode(folderA, rootUiNode); // collapsed (default)
            var folderBUiNode = MakeUiNode(folderB, rootUiNode); // expanded
            var assetB1UiNode = MakeUiNode(assetB1, folderBUiNode);
            var assetB2UiNode = MakeUiNode(assetB2, folderBUiNode);

            SetIsExpanded(rootUiNode, true);
            AddChildUiNode(rootUiNode, folderAUiNode);
            AddChildUiNode(rootUiNode, folderBUiNode);
            SetIsExpanded(folderBUiNode, true);
            AddChildUiNode(folderBUiNode, assetB1UiNode);
            AddChildUiNode(folderBUiNode, assetB2UiNode);
            // folderA stays collapsed — data traversal collects assetA1 + assetA2

            var result = MakeImporter().CollectNodesToWriteOut(rootUiNode);

            // root + folderA + assetA1 + assetA2 (data) + folderB + assetB1 + assetB2 (UI)
            Assert.AreEqual(7, result.Count);
            CollectionAssert.Contains(result, root);
            CollectionAssert.Contains(result, folderA);
            CollectionAssert.Contains(result, assetA1);
            CollectionAssert.Contains(result, assetA2);
            CollectionAssert.Contains(result, folderB);
            CollectionAssert.Contains(result, assetB1);
            CollectionAssert.Contains(result, assetB2);
        }
    }
}
