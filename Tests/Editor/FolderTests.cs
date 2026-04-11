using HamerSoft.Victoria.Core.Extractor.Nodes;
using NUnit.Framework;

namespace HamerSoft.Victoria.Tests.Editor
{
    [TestFixture]
    public class FolderTests
    {
        // -----------------------------------------------------------------------
        // Construction
        // -----------------------------------------------------------------------

        [Test]
        public void Constructor_SetsName()
        {
            var folder = new Folder("Scripts");
            Assert.AreEqual("Scripts", folder.Name);
        }

        [Test]
        public void Constructor_SetsPathToName()
        {
            var folder = new Folder("Scripts");
            Assert.AreEqual("Scripts", folder.Path);
        }

        [Test]
        public void Constructor_ChildrenIsEmpty()
        {
            var folder = new Folder("Scripts");
            Assert.AreEqual(0, folder.Children.Count);
        }

        // -----------------------------------------------------------------------
        // IsLeaf / HasChildren
        // -----------------------------------------------------------------------

        [Test]
        public void IsLeaf_IsAlwaysFalse()
        {
            var folder = new Folder("Scripts");
            Assert.IsFalse(folder.IsLeaf);
        }

        [Test]
        public void HasChildren_FalseOnNewFolder()
        {
            var folder = new Folder("Scripts");
            Assert.IsFalse(folder.HasChildren);
        }

        [Test]
        public void HasChildren_TrueAfterAddingChildFolder()
        {
            var parent = new Folder("Parent");
            parent.AddChild(new Folder("Child"));
            Assert.IsTrue(parent.HasChildren);
        }

        [Test]
        public void HasChildren_TrueAfterAddingChildAsset()
        {
            var parent = new Folder("Parent");
            parent.AddChild(new Asset { Name = "Script", ContentType = ".cs" });
            Assert.IsTrue(parent.HasChildren);
        }

        // -----------------------------------------------------------------------
        // DetailedName / FullPath / ToString
        // -----------------------------------------------------------------------

        [Test]
        public void DetailedName_ReturnsName()
        {
            var folder = new Folder("Scripts");
            Assert.AreEqual("Scripts", folder.DetailedName);
        }

        [Test]
        public void FullPath_ReturnsPath()
        {
            var folder = new Folder("Scripts");
            Assert.AreEqual(folder.Path, folder.FullPath);
        }

        [Test]
        public void ToString_ReturnsName()
        {
            var folder = new Folder("Scripts");
            Assert.AreEqual("Scripts", folder.ToString());
        }

        // -----------------------------------------------------------------------
        // AddChild — parent assignment and path resolution
        // -----------------------------------------------------------------------

        [Test]
        public void AddChild_Folder_SetsParent()
        {
            var parent = new Folder("Parent");
            var child = new Folder("Child");
            parent.AddChild(child);
            Assert.AreEqual(parent, child.Parent);
        }

        [Test]
        public void AddChild_Asset_SetsParent()
        {
            var parent = new Folder("Parent");
            var asset = new Asset { Name = "Script", ContentType = ".cs" };
            parent.AddChild(asset);
            Assert.AreEqual(parent, asset.Parent);
        }

        [Test]
        public void AddChild_Folder_ResolvesChildPath()
        {
            var parent = new Folder("Parent");
            var child = new Folder("Child");
            parent.AddChild(child);
            Assert.AreEqual("Parent/Child", child.Path);
        }

        [Test]
        public void AddChild_Asset_ResolvesAssetPath()
        {
            var parent = new Folder("Parent");
            var asset = new Asset { Name = "Script", ContentType = ".cs" };
            parent.AddChild(asset);
            Assert.AreEqual("Parent/Script", asset.Path);
        }

        [Test]
        public void AddChild_DeepHierarchy_ResolvesFullPath()
        {
            var root = new Folder("Root");
            var middle = new Folder("Middle");
            var leaf = new Asset { Name = "Script", ContentType = ".cs" };
            root.AddChild(middle);
            middle.AddChild(leaf);
            Assert.AreEqual("Root/Middle/Script", leaf.Path);
        }

        [Test]
        public void AddChild_IncreasesChildrenCount()
        {
            var parent = new Folder("Parent");
            parent.AddChild(new Folder("A"));
            parent.AddChild(new Folder("B"));
            Assert.AreEqual(2, parent.Children.Count);
        }

        // -----------------------------------------------------------------------
        // AddChild — HashSet deduplication (same name, same type)
        // -----------------------------------------------------------------------

        [Test]
        public void AddChild_DuplicateFolderName_IsIgnored()
        {
            var parent = new Folder("Parent");
            parent.AddChild(new Folder("Child"));
            parent.AddChild(new Folder("Child"));
            Assert.AreEqual(1, parent.Children.Count);
        }

        [Test]
        public void AddChild_DuplicateAssetName_IsIgnored()
        {
            var parent = new Folder("Parent");
            parent.AddChild(new Asset { Name = "Script", ContentType = ".cs" });
            parent.AddChild(new Asset { Name = "Script", ContentType = ".cs" });
            Assert.AreEqual(1, parent.Children.Count);
        }

        [Test]
        public void AddChild_FolderAndAssetWithSameName_BothAdded()
        {
            // NodeEqualityComparer checks type, so Folder("X") != Asset{Name="X"}
            var parent = new Folder("Parent");
            parent.AddChild(new Folder("Shared"));
            parent.AddChild(new Asset { Name = "Shared", ContentType = ".cs" });
            Assert.AreEqual(2, parent.Children.Count);
        }

        // -----------------------------------------------------------------------
        // TryGetChild
        // -----------------------------------------------------------------------

        [Test]
        public void TryGetChild_ExistingFolder_ReturnsTrueAndFolder()
        {
            var parent = new Folder("Parent");
            var child = new Folder("Child");
            parent.AddChild(child);

            var found = parent.TryGetChild("Child", out var result);

            Assert.IsTrue(found);
            Assert.AreEqual(child, result);
        }

        [Test]
        public void TryGetChild_MissingName_ReturnsFalse()
        {
            var parent = new Folder("Parent");
            parent.AddChild(new Folder("Child"));

            var found = parent.TryGetChild("Other", out var result);

            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        [Test]
        public void TryGetChild_EmptyFolder_ReturnsFalse()
        {
            var parent = new Folder("Parent");
            var found = parent.TryGetChild("Anything", out var result);
            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        [Test]
        public void TryGetChild_NameExistsAsAssetNotFolder_ReturnsFalse()
        {
            // TryGetChild only returns Folder nodes; an Asset with the same name
            // should not be returned.
            var parent = new Folder("Parent");
            parent.AddChild(new Asset { Name = "Scripts", ContentType = ".cs" });

            var found = parent.TryGetChild("Scripts", out var result);

            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        [Test]
        public void TryGetChild_MultipleChildren_ReturnsCorrectOne()
        {
            var parent = new Folder("Parent");
            var a = new Folder("A");
            var b = new Folder("B");
            var c = new Folder("C");
            parent.AddChild(a);
            parent.AddChild(b);
            parent.AddChild(c);

            parent.TryGetChild("B", out var result);

            Assert.AreEqual(b, result);
        }
    }
}
