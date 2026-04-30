using HamerSoft.Victoria.Core.Extractor.Nodes;
using NUnit.Framework;

namespace HamerSoft.Victoria.Tests.Editor
{
    [TestFixture]
    public class NodePathResolutionTests
    {
        [Test]
        public void RootFolder_PathEqualsName()
        {
            var root = new Folder("Root");
            Assert.AreEqual("Root", root.Path);
        }

        [Test]
        public void RootFolder_EmptyName_PathIsEmpty()
        {
            var root = new Folder("");
            Assert.AreEqual("", root.Path);
        }

        [Test]
        public void AddChild_Folder_PathIsParentSlashChild()
        {
            var parent = new Folder("Parent");
            var child = new Folder("Child");
            parent.AddChild(child);
            Assert.AreEqual("Parent/Child", child.Path);
        }

        [Test]
        public void AddChild_Asset_PathIsParentSlashName()
        {
            var parent = new Folder("Parent");
            var asset = new Asset { Name = "Script", ContentType = ".cs" };
            parent.AddChild(asset);
            Assert.AreEqual("Parent/Script", asset.Path);
        }

        [Test]
        public void AddChild_DoesNotChangeParentPath()
        {
            var parent = new Folder("Parent");
            parent.AddChild(new Folder("Child"));
            Assert.AreEqual("Parent", parent.Path);
        }

        [Test]
        public void ThreeLevels_Folder_PathIncludesAllAncestors()
        {
            var root = new Folder("Root");
            var middle = new Folder("Middle");
            var leaf = new Folder("Leaf");
            root.AddChild(middle);
            middle.AddChild(leaf);
            Assert.AreEqual("Root/Middle/Leaf", leaf.Path);
        }

        [Test]
        public void ThreeLevels_Asset_PathIncludesAllAncestors()
        {
            var root = new Folder("Root");
            var middle = new Folder("Middle");
            var asset = new Asset { Name = "Script", ContentType = ".cs" };
            root.AddChild(middle);
            middle.AddChild(asset);
            Assert.AreEqual("Root/Middle/Script", asset.Path);
        }

        [Test]
        public void FourLevels_PathIncludesAllAncestors()
        {
            var a = new Folder("A");
            var b = new Folder("B");
            var c = new Folder("C");
            var d = new Folder("D");
            a.AddChild(b);
            b.AddChild(c);
            c.AddChild(d);
            Assert.AreEqual("A/B/C/D", d.Path);
        }

        [Test]
        public void Siblings_HaveIndependentPaths()
        {
            var parent = new Folder("Parent");
            var a = new Folder("A");
            var b = new Folder("B");
            parent.AddChild(a);
            parent.AddChild(b);
            Assert.AreEqual("Parent/A", a.Path);
            Assert.AreEqual("Parent/B", b.Path);
        }

        [Test]
        public void AddChildWithExistingDescendants_RePathsAllDescendants()
        {
            var root = new Folder("Root");
            var middle = new Folder("Middle");
            var leaf = new Folder("Leaf");
            middle.AddChild(leaf);
            root.AddChild(middle);

            Assert.AreEqual("Root/Middle", middle.Path);
            Assert.AreEqual("Root/Middle/Leaf", leaf.Path);
        }

        [Test]
        public void AddChildWithExistingDescendants_RePathsAssetLeaf()
        {
            var root = new Folder("Root");
            var middle = new Folder("Middle");
            var asset = new Asset { Name = "Script", ContentType = ".cs" };
            middle.AddChild(asset);
            root.AddChild(middle);
            Assert.AreEqual("Root/Middle/Script", asset.Path);
        }

        [Test]
        public void ReparentNode_UpdatesPathForNodeAndDescendants()
        {
            var p1 = new Folder("P1");
            var p2 = new Folder("P2");
            var child = new Folder("Child");
            var grandchild = new Folder("Grandchild");
            child.AddChild(grandchild);
            p1.AddChild(child);

            Assert.AreEqual("P1/Child", child.Path);
            Assert.AreEqual("P1/Child/Grandchild", grandchild.Path);

            child.Parent = p2;

            Assert.AreEqual("P2/Child", child.Path);
            Assert.AreEqual("P2/Child/Grandchild", grandchild.Path);
        }

        [Test]
        public void ParentWithEmptyName_IsNotIncludedInPath()
        {
            var anonymousRoot = new Folder("");
            var child = new Folder("Child");
            anonymousRoot.AddChild(child);
            Assert.AreEqual("Child", child.Path);
        }

        [Test]
        public void ParentWithEmptyName_GrandchildPathStartsFromNamedAncestor()
        {
            var anonymousRoot = new Folder("");
            var middle = new Folder("Middle");
            var leaf = new Folder("Leaf");
            anonymousRoot.AddChild(middle);
            middle.AddChild(leaf);
            Assert.AreEqual("Middle", middle.Path);
            Assert.AreEqual("Middle/Leaf", leaf.Path);
        }

        [Test]
        public void Folder_FullPath_MatchesPath()
        {
            var parent = new Folder("Parent");
            var child = new Folder("Child");
            parent.AddChild(child);
            Assert.AreEqual(child.Path, child.FullPath);
        }

        [Test]
        public void Asset_FullPath_IsPathPlusContentType()
        {
            var parent = new Folder("Parent");
            var asset = new Asset { Name = "Script", ContentType = ".cs" };
            parent.AddChild(asset);
            Assert.AreEqual("Parent/Script.cs", asset.FullPath);
        }
    }
}
