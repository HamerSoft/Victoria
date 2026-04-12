using System.Collections.Generic;
using System.Linq;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Core.Search;
using NUnit.Framework;

namespace HamerSoft.Victoria.Tests.Editor
{
    [TestFixture]
    public class SearcherTests
    {
        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        /// <summary>Builds a root Folder and wires up a Searcher against it.</summary>
        private static (Folder root, Searcher searcher) MakeTree()
        {
            var root = new Folder("Root");
            return (root, new Searcher(root));
        }

        private static Asset MakeAsset(string name, string extension)
            => new Asset { Name = name, ContentType = extension, FileContent = new byte[0] };

        // -----------------------------------------------------------------------
        // 5.1 — Exact name match
        // -----------------------------------------------------------------------

        [Test]
        public void SearchByName_ExactNameMatch_ReturnsMatchingNode()
        {
            var (root, searcher) = MakeTree();
            var asset = MakeAsset("MyScript", ".cs");
            root.AddChild(asset);

            var results = searcher.SearchByName("MyScript.cs").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(asset, results[0]);
        }

        // -----------------------------------------------------------------------
        // 5.2 — Substring match
        // -----------------------------------------------------------------------

        [Test]
        public void SearchByName_SubstringMatch_ReturnsMatchingNodes()
        {
            var (root, searcher) = MakeTree();
            var a = MakeAsset("PlayerController", ".cs");
            var b = MakeAsset("EnemyController", ".cs");
            var c = MakeAsset("Unrelated", ".txt");
            root.AddChild(a);
            root.AddChild(b);
            root.AddChild(c);

            var results = searcher.SearchByName("Controller").ToList();

            Assert.AreEqual(2, results.Count);
            CollectionAssert.Contains(results, a);
            CollectionAssert.Contains(results, b);
        }

        // -----------------------------------------------------------------------
        // 5.3 — Case-insensitive match
        // -----------------------------------------------------------------------

        [Test]
        public void SearchByName_CaseInsensitiveMatch_ReturnsNode()
        {
            var (root, searcher) = MakeTree();
            var asset = MakeAsset("MyAsset", ".cs");
            root.AddChild(asset);

            var results = searcher.SearchByName("myasset.cs").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(asset, results[0]);
        }

        [Test]
        public void SearchByName_UpperCaseTerm_ReturnsNode()
        {
            var (root, searcher) = MakeTree();
            var asset = MakeAsset("data", ".json");
            root.AddChild(asset);

            var results = searcher.SearchByName("DATA.JSON").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(asset, results[0]);
        }

        // -----------------------------------------------------------------------
        // 5.4 — No match
        // -----------------------------------------------------------------------

        [Test]
        public void SearchByName_NoMatch_ReturnsEmptyCollection()
        {
            var (root, searcher) = MakeTree();
            root.AddChild(MakeAsset("PlayerController", ".cs"));

            var results = searcher.SearchByName("NonExistent").ToList();

            Assert.IsEmpty(results);
        }

        // -----------------------------------------------------------------------
        // 5.5 — Empty search term returns all nodes
        // -----------------------------------------------------------------------

        [Test]
        public void SearchByName_EmptyTerm_ReturnsAllNodes()
        {
            var (root, searcher) = MakeTree();
            var a = MakeAsset("Alpha", ".cs");
            var b = MakeAsset("Beta", ".txt");
            var sub = new Folder("Sub");
            var c = MakeAsset("Gamma", ".json");
            sub.AddChild(c);
            root.AddChild(a);
            root.AddChild(b);
            root.AddChild(sub);

            var results = searcher.SearchByName("").ToList();

            // Sub folder + a + b + c
            Assert.AreEqual(4, results.Count);
            CollectionAssert.Contains(results, a);
            CollectionAssert.Contains(results, b);
            CollectionAssert.Contains(results, sub);
            CollectionAssert.Contains(results, c);
        }

        // -----------------------------------------------------------------------
        // 5.6 — Nested match (depth > 1)
        // -----------------------------------------------------------------------

        [Test]
        public void SearchByName_NestedAsset_IsFound()
        {
            var (root, searcher) = MakeTree();
            var sub = new Folder("Sub");
            var nested = MakeAsset("DeepAsset", ".cs");
            sub.AddChild(nested);
            root.AddChild(sub);

            var results = searcher.SearchByName("DeepAsset.cs").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(nested, results[0]);
        }

        [Test]
        public void SearchByName_ThreeLevelsDeep_IsFound()
        {
            var (root, searcher) = MakeTree();
            var level1 = new Folder("Level1");
            var level2 = new Folder("Level2");
            var deep = MakeAsset("VeryDeep", ".cs");
            level2.AddChild(deep);
            level1.AddChild(level2);
            root.AddChild(level1);

            var results = searcher.SearchByName("VeryDeep.cs").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(deep, results[0]);
        }

        // -----------------------------------------------------------------------
        // 5.7 — Multiple matches
        // -----------------------------------------------------------------------

        [Test]
        public void SearchByName_MultipleMatches_ReturnsAllMatchingNodes()
        {
            var (root, searcher) = MakeTree();
            var a = MakeAsset("Spawn", ".cs");
            var b = MakeAsset("SpawnEffect", ".cs");
            var c = MakeAsset("Unrelated", ".txt");
            var sub = new Folder("Sub");
            var d = MakeAsset("SpawnPoint", ".prefab");
            sub.AddChild(d);
            root.AddChild(a);
            root.AddChild(b);
            root.AddChild(c);
            root.AddChild(sub);

            var results = searcher.SearchByName("Spawn").ToList();

            Assert.AreEqual(3, results.Count);
            CollectionAssert.Contains(results, a);
            CollectionAssert.Contains(results, b);
            CollectionAssert.Contains(results, d);
        }

        // -----------------------------------------------------------------------
        // Additional edge cases
        // -----------------------------------------------------------------------

        [Test]
        public void SearchByName_EmptyTree_ReturnsEmptyCollection()
        {
            var (_, searcher) = MakeTree();

            var results = searcher.SearchByName("Anything").ToList();

            Assert.IsEmpty(results);
        }

        [Test]
        public void SearchByName_FolderNameMatchesTerm_FolderIsIncluded()
        {
            var (root, searcher) = MakeTree();
            var scripts = new Folder("Scripts");
            root.AddChild(scripts);

            var results = searcher.SearchByName("Scripts").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(scripts, results[0]);
        }
    }
}
