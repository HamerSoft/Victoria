using System;
using System.Collections.Generic;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Ui.Elements.Nodes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Tests.Editor
{
    [TestFixture]
    public class BaseUiNodeKeyboardNavigationTests
    {
        private TestEditorWindow _window;
        private ScrollView _scrollView;

        [SetUp]
        public void SetUp()
        {
            _window = EditorWindow.CreateWindow<TestEditorWindow>();
            _scrollView = new ScrollView();
            _window.rootVisualElement.Add(_scrollView);
        }

        [TearDown]
        public void TearDown()
        {
            _window.Close();
        }

        // ---- Helpers ----

        private TestUiNode MakeAndAddNode(Node node, TestUiNode parent = null)
        {
            var uiNode = new TestUiNode(node, 0, parent);
            if (parent == null)
            {
                _scrollView.Add(uiNode);
                uiNode.RegisterScrollView(_scrollView);
            }

            return uiNode;
        }

        private static List<BaseUiNode> ChildrenOf(BaseUiNode node) => node.ChildUiNodes;

        private static Asset MakeAsset(string name = "Asset") =>
            new Asset { Name = name, ContentType = ".cs", FileContent = Array.Empty<byte>() };

        private static void SendKey(BaseUiNode node, KeyCode key)
        {
            using var evt = KeyDownEvent.GetPooled('\0', key, EventModifiers.None);
            node.SendEvent(evt);
        }

        private Focusable FocusedElement =>
            _window.rootVisualElement.focusController.focusedElement;

        [Test]
        public void DownArrow_CollapsedLeafWithNextSibling_FocusesNextSibling()
        {
            var parentData = new Folder("Parent");
            parentData.AddChild(MakeAsset("Child1"));
            parentData.AddChild(MakeAsset("Child2"));

            var parentNode = MakeAndAddNode(parentData);
            parentNode.ExpandOrCollapse();

            var children = ChildrenOf(parentNode);
            children[0].Focus();
            SendKey(children[0], KeyCode.DownArrow);

            Assert.AreEqual(children[1], FocusedElement);
        }

        [Test]
        public void DownArrow_LastLeafInParent_FocusesUncle()
        {
            var rootData = new Folder("Root");
            var folderAData = new Folder("FolderA");
            folderAData.AddChild(MakeAsset("Child1"));
            rootData.AddChild(folderAData);
            rootData.AddChild(new Folder("FolderB"));

            var rootNode = MakeAndAddNode(rootData);
            rootNode.ExpandOrCollapse();

            var rootChildren = ChildrenOf(rootNode);
            var folderANode = rootChildren[0];
            var folderBNode = rootChildren[1];

            folderANode.ExpandOrCollapse();
            var deepChild = ChildrenOf(folderANode)[0];

            deepChild.Focus();
            SendKey(deepChild, KeyCode.DownArrow);

            Assert.AreEqual(folderBNode, FocusedElement);
        }

        [Test]
        public void DownArrow_ExpandedFolder_FocusesFirstChild()
        {
            var rootData = new Folder("Root");
            var folderData = new Folder("Folder");
            folderData.AddChild(MakeAsset("Child"));
            rootData.AddChild(folderData);

            var rootNode = MakeAndAddNode(rootData);
            rootNode.ExpandOrCollapse();

            var folderNode = ChildrenOf(rootNode)[0];
            folderNode.ExpandOrCollapse();
            var childNode = ChildrenOf(folderNode)[0];

            folderNode.Focus();
            SendKey(folderNode, KeyCode.DownArrow);

            Assert.AreEqual(childNode, FocusedElement);
        }

        [Test]
        public void DownArrow_BottomOfTree_FocusDoesNotChange()
        {
            var rootData = new Folder("Root");
            rootData.AddChild(MakeAsset("Only"));

            var rootNode = MakeAndAddNode(rootData);
            rootNode.ExpandOrCollapse();

            var childNode = ChildrenOf(rootNode)[0];
            childNode.Focus();
            SendKey(childNode, KeyCode.DownArrow);

            Assert.AreEqual(childNode, FocusedElement);
        }

        [Test]
        public void UpArrow_PreviousSiblingIsExpanded_FocusesDeepestLastChild()
        {
            var rootData = new Folder("Root");
            var folderAData = new Folder("FolderA");
            folderAData.AddChild(MakeAsset("DeepChild"));
            rootData.AddChild(folderAData);
            rootData.AddChild(MakeAsset("LeafB"));

            var rootNode = MakeAndAddNode(rootData);
            rootNode.ExpandOrCollapse();

            var rootChildren = ChildrenOf(rootNode);
            var folderANode = rootChildren[0];
            var leafBNode = rootChildren[1];

            folderANode.ExpandOrCollapse();
            var deepChild = ChildrenOf(folderANode)[0];

            leafBNode.Focus();
            SendKey(leafBNode, KeyCode.UpArrow);

            Assert.AreEqual(deepChild, FocusedElement);
        }

        [Test]
        public void UpArrow_FirstChild_FocusesParent()
        {
            var rootData = new Folder("Root");
            rootData.AddChild(MakeAsset("Child"));

            var rootNode = MakeAndAddNode(rootData);
            rootNode.ExpandOrCollapse();

            var childNode = ChildrenOf(rootNode)[0];
            childNode.Focus();
            SendKey(childNode, KeyCode.UpArrow);

            Assert.AreEqual(rootNode, FocusedElement);
        }

        [Test]
        public void UpArrow_RootNode_FocusDoesNotChange()
        {
            var rootNode = MakeAndAddNode(new Folder("Root"));
            rootNode.Focus();
            SendKey(rootNode, KeyCode.UpArrow);

            Assert.AreEqual(rootNode, FocusedElement);
        }

        [Test]
        public void RightArrow_CollapsedFolder_ExpandsAndFocusesFirstChild()
        {
            var rootData = new Folder("Root");
            var folderData = new Folder("Folder");
            folderData.AddChild(MakeAsset("Child"));
            rootData.AddChild(folderData);

            var rootNode = MakeAndAddNode(rootData);
            rootNode.ExpandOrCollapse();

            var folderNode = ChildrenOf(rootNode)[0];
            folderNode.Focus();
            SendKey(folderNode, KeyCode.RightArrow);

            Assert.IsTrue(folderNode.IsExpanded);
            Assert.AreEqual(ChildrenOf(folderNode)[0], FocusedElement);
        }

        [Test]
        public void RightArrow_LeafNode_FocusDoesNotChange()
        {
            var rootData = new Folder("Root");
            rootData.AddChild(MakeAsset("Leaf"));

            var rootNode = MakeAndAddNode(rootData);
            rootNode.ExpandOrCollapse();

            var leafNode = ChildrenOf(rootNode)[0];
            leafNode.Focus();
            SendKey(leafNode, KeyCode.RightArrow);

            Assert.AreEqual(leafNode, FocusedElement);
        }

        [Test]
        public void LeftArrow_ExpandedFolder_Collapses()
        {
            var rootData = new Folder("Root");
            var folderData = new Folder("Folder");
            folderData.AddChild(MakeAsset("Child"));
            rootData.AddChild(folderData);

            var rootNode = MakeAndAddNode(rootData);
            rootNode.ExpandOrCollapse();

            var folderNode = ChildrenOf(rootNode)[0];
            folderNode.ExpandOrCollapse();

            folderNode.Focus();
            SendKey(folderNode, KeyCode.LeftArrow);

            Assert.IsFalse(folderNode.IsExpanded);
        }

        [Test]
        public void LeftArrow_CollapsedNode_FocusesParent()
        {
            var rootData = new Folder("Root");
            var folderData = new Folder("Folder");
            folderData.AddChild(MakeAsset("Child"));
            rootData.AddChild(folderData);

            var rootNode = MakeAndAddNode(rootData);
            rootNode.ExpandOrCollapse();

            var folderNode = ChildrenOf(rootNode)[0];
            folderNode.Focus();
            SendKey(folderNode, KeyCode.LeftArrow);

            Assert.AreEqual(rootNode, FocusedElement);
        }

        [Test]
        public void HandledKey_StopsPropagation()
        {
            var rootData = new Folder("Root");
            rootData.AddChild(MakeAsset("Child"));

            var rootNode = MakeAndAddNode(rootData);
            rootNode.ExpandOrCollapse();
            var childNode = ChildrenOf(rootNode)[0];

            var propagated = false;
            _scrollView.RegisterCallback<KeyDownEvent>(_ => propagated = true);

            childNode.Focus();
            SendKey(childNode, KeyCode.DownArrow);

            Assert.IsFalse(propagated);
        }

        [Test]
        public void UnhandledKey_DoesNotStopPropagation()
        {
            var rootData = new Folder("Root");
            rootData.AddChild(MakeAsset("Child"));

            var rootNode = MakeAndAddNode(rootData);
            rootNode.ExpandOrCollapse();
            var childNode = ChildrenOf(rootNode)[0];

            var propagated = false;
            _scrollView.RegisterCallback<KeyDownEvent>(_ => propagated = true);

            childNode.Focus();
            SendKey(childNode, KeyCode.Space);

            Assert.IsTrue(propagated);
        }

        private class TestEditorWindow : EditorWindow
        {
        }

        private class TestUiNode : BaseUiNode
        {
            public TestUiNode(Node node, int depth, BaseUiNode parent) : base(node, depth, parent)
            {
            }

            protected override BaseUiNode CreateNode(Node node) => new TestUiNode(node, Depth + 1, this);
        }
    }
}