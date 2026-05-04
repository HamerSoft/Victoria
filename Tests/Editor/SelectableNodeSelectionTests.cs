using System.Collections.Generic;
using System.Reflection;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Ui.Elements.Nodes;
using HamerSoft.Victoria.Ui.SleurEnPleur;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Tests.Editor
{
    [TestFixture]
    public class SelectableNodeSelectionTests
    {
        private static readonly FieldInfo IsExpandedField =
            typeof(BaseUiNode).GetField("_isExpanded", BindingFlags.NonPublic | BindingFlags.Instance);

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

        private SelectableNode MakeRootNode(Node node)
        {
            var uiNode = new SelectableNode(node, 0, (VisualElement)_scrollView);
            _scrollView.Add(uiNode);
            uiNode.RegisterScrollView(_scrollView);
            return uiNode;
        }

        private SelectableNode MakeChildNode(Node node, SelectableNode parent)
        {
            var child = new SelectableNode(node, 1, parent);
            parent.contentContainer.Add(child);
            parent.ChildUiNodes.Add(child);
            child.RegisterScrollView(_scrollView);
            return child;
        }

        private static Toggle GetToggle(SelectableNode node) =>
            node.Toggle;

        private static void SetIsExpanded(BaseUiNode node, bool value) =>
            IsExpandedField.SetValue(node, value);

        private static Asset MakeAsset(string name = "Asset") =>
            new Asset { Name = name, ContentType = ".cs", FileContent = new byte[0] };

        private static void SendKey(VisualElement element, KeyCode key)
        {
            using var evt = KeyDownEvent.GetPooled('\0', key, EventModifiers.None);
            element.SendEvent(evt);
        }

        // ---- Tests ----

        // 2.1
        [Test]
        public void ToggleParentOn_AllChildrenBecomeSelected()
        {
            var rootData = new Folder("Root");
            var asset1 = MakeAsset("Child1");
            var asset2 = MakeAsset("Child2");
            rootData.AddChild(asset1);
            rootData.AddChild(asset2);

            var rootNode = MakeRootNode(rootData);
            GetToggle(rootNode).value = false;

            var child1 = MakeChildNode(asset1, rootNode);
            var child2 = MakeChildNode(asset2, rootNode);
            SetIsExpanded(rootNode, true);

            GetToggle(rootNode).value = true;

            Assert.IsTrue(child1.IsSelected);
            Assert.IsTrue(child2.IsSelected);
        }

        // 2.2
        [Test]
        public void ToggleParentOff_AllChildrenBecomeDeselected()
        {
            var rootData = new Folder("Root");
            var asset1 = MakeAsset("Child1");
            var asset2 = MakeAsset("Child2");
            rootData.AddChild(asset1);
            rootData.AddChild(asset2);

            var rootNode = MakeRootNode(rootData);
            var child1 = MakeChildNode(asset1, rootNode);
            var child2 = MakeChildNode(asset2, rootNode);
            SetIsExpanded(rootNode, true);

            GetToggle(rootNode).value = false;

            Assert.IsFalse(child1.IsSelected);
            Assert.IsFalse(child2.IsSelected);
        }

        // 2.3
        [Test]
        public void ToggleChildOn_WhenParentIsOff_ParentToggleReflectsChange()
        {
            var rootData = new Folder("Root");
            var asset = MakeAsset("Child");
            rootData.AddChild(asset);

            var rootNode = MakeRootNode(rootData);
            GetToggle(rootNode).value = false;

            var child = MakeChildNode(asset, rootNode);
            SetIsExpanded(rootNode, true);

            GetToggle(child).value = true;

            Assert.IsTrue(GetToggle(rootNode).value);
        }

        // 2.4
        [Test]
        public void ReturnKey_OnSelectedNode_InvertsSelectionRecursively()
        {
            var rootData = new Folder("Root");
            var asset1 = MakeAsset("Child1");
            var asset2 = MakeAsset("Child2");
            rootData.AddChild(asset1);
            rootData.AddChild(asset2);

            var rootNode = MakeRootNode(rootData);
            var child1 = MakeChildNode(asset1, rootNode);
            var child2 = MakeChildNode(asset2, rootNode);
            SetIsExpanded(rootNode, true);

            rootNode.Focus();
            SendKey(rootNode, KeyCode.Return);

            Assert.IsFalse(rootNode.IsSelected);
            Assert.IsFalse(child1.IsSelected);
            Assert.IsFalse(child2.IsSelected);
        }

        // 2.5
        [Test]
        public void ReturnKey_StopsPropagation()
        {
            var rootData = new Folder("Root");
            rootData.AddChild(MakeAsset("Child"));

            var rootNode = MakeRootNode(rootData);

            var propagated = false;
            _scrollView.RegisterCallback<KeyDownEvent>(_ => propagated = true);

            rootNode.Focus();
            SendKey(rootNode, KeyCode.Return);

            Assert.IsFalse(propagated);
        }

        // ---- Nested helpers ----

        private class TestEditorWindow : EditorWindow
        {
        }

        private class NullDragParent : IDragParent
        {
            void IDragParent.Add(BaseUiNode uiNode)
            {
            }
        }
    }
}