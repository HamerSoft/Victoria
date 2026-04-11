using System;
using System.Collections.Generic;
using System.Linq;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui.Elements.Nodes
{
    public abstract class BaseUiNode : VisualElement
    {
        private const int UI_NODE_HEADER_HEIGHT = 20;
        private readonly VisualElement _contentContainer;
        protected BaseUiNode ParentUiNode;
        internal readonly Node Node;
        protected readonly Label Label;
        public readonly int OriginalDepth;
        private bool IsLocked => _lockedCounter > 0;

        private bool _isExpanded;
        protected int Depth;
        internal ScrollView ParentScrollView { get; private set; }
        private int _lockedCounter;
        protected virtual int DepthMultiplier { get; } = 2;
        protected List<BaseUiNode> ChildUiNodes { get; private set; } = new();
        protected VisualElement Header { get; }

        public override VisualElement contentContainer => _contentContainer ?? base.contentContainer;
        public bool IsLeaf => Node.IsLeaf;
        public bool IsExpanded => _isExpanded;

        internal static event Action<BaseUiNode, Node> Focussed;

        public BaseUiNode(Node node, int depth, BaseUiNode parentNode)
        {
            name = node.Name;
            focusable = true;
            ParentUiNode = parentNode;
            Node = node;
            Depth = OriginalDepth = depth;
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
            DepthMultiplier = 2;
            style.marginLeft = Depth * DepthMultiplier;

            Header = new VisualElement
            {
                name = "header",
                style =
                {
                    height = 20,
                    flexGrow = 1,
                    alignContent = new StyleEnum<Align>(Align.FlexStart),
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                }
            };

            Header.Add(Label = new Label(node.Name)
            {
                pickingMode = PickingMode.Ignore
            });
            Add(Header);
            _contentContainer = BuildContentContainer();

            if (!node.IsLeaf)
            {
                SetLabelText(_isExpanded);
                Header.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.4f);
                Label.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
                Label.style.backgroundColor = Color.clear;
                Header.RegisterCallback<PointerUpEvent>(_ => { ExpandOrCollapse(); });
            }
            else
            {
                Label.text = node.DetailedName;
            }

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            var normalColor = Color.clear;
            var focusColor = new Color(0.235f, 0.55f, 1f, 0.25f); // Soft blue, 25% opacity

            Header.style.backgroundColor = normalColor;
            RegisterCallback<FocusEvent>(OnFocus(focusColor));
            RegisterCallback<BlurEvent>(_ => { Header.style.backgroundColor = normalColor; });

            RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (focusController.focusedElement != this)
                    Header.style.backgroundColor = new Color(1f, 1f, 1f, 0.08f); // very light hover
            });
            RegisterCallback<PointerLeaveEvent>(_ =>
            {
                if (focusController.focusedElement != this)
                    Header.style.backgroundColor = normalColor;
            });
        }

        private void OnKeyDown(KeyDownEvent keyDownEvent)
        {
            if (focusController.focusedElement != this) return;
            int indexInParent;

            switch (keyDownEvent.keyCode)
            {
                case KeyCode.DownArrow:
                    var currentParentNode = ParentUiNode;
                    var currentUiNode = this;
                    var currentNode = Node;
                    if (currentParentNode == null)
                    {
                        if (_isExpanded && Node.HasChildren) currentUiNode.ChildUiNodes[0].Focus();

                        break;
                    }

                    if (currentUiNode._isExpanded && currentNode.HasChildren && currentUiNode.ChildUiNodes.Count > 0)
                    {
                        currentUiNode.ChildUiNodes[0].Focus();
                        break;
                    }

                    indexInParent = currentParentNode.ChildUiNodes.IndexOf(currentUiNode);
                    if (indexInParent < currentParentNode.ChildUiNodes.Count - 1)
                    {
                        currentParentNode.ChildUiNodes[indexInParent + 1].Focus();
                        break;
                    }

                    while (currentParentNode is { ChildUiNodes: { Count: > 0 } })
                    {
                        currentUiNode = currentParentNode;
                        currentNode = currentNode.Parent;
                        currentParentNode = currentParentNode.ParentUiNode;
                        if (currentUiNode == null || currentParentNode == null) break;
                        indexInParent = currentParentNode.ChildUiNodes.IndexOf(currentUiNode);
                        if (indexInParent < currentParentNode.ChildUiNodes.Count - 1)
                        {
                            currentParentNode.ChildUiNodes[indexInParent + 1].Focus();
                            break;
                        }
                    }

                    break;
                case KeyCode.UpArrow:
                    if (ParentUiNode == null) return;
                    indexInParent = ParentUiNode.ChildUiNodes.IndexOf(this);
                    if (indexInParent > 0)
                    {
                        var sibling = ParentUiNode.ChildUiNodes[indexInParent - 1];
                        while (sibling._isExpanded)
                        {
                            if (sibling.ChildUiNodes.Count == 0) break;
                            sibling = sibling.ChildUiNodes.Last();
                        }

                        sibling.Focus();
                    }
                    else
                    {
                        ParentUiNode.Focus();
                    }

                    break;
                case KeyCode.LeftArrow:
                    if (_isExpanded && !IsLocked)
                    {
                        Collapse();
                        _isExpanded = false;
                        SetLabelText(_isExpanded);
                    }
                    else
                    {
                        ParentUiNode?.Focus();
                    }

                    break;
                case KeyCode.RightArrow:
                    if (Node.HasChildren)
                    {
                        if (!_isExpanded)
                        {
                            Expand();
                            _isExpanded = true;
                            SetLabelText(_isExpanded);
                        }

                        if (ChildUiNodes.Count > 0) ChildUiNodes[0]?.Focus();
                    }

                    break;
                default:
                    return;
            }
        }

        internal void ExpandOrCollapse()
        {
            if (!Node.HasChildren)
                return;

            if (_isExpanded)
                Collapse();
            else
                Expand();

            _isExpanded = !_isExpanded;
            SetLabelText(_isExpanded);
            Focus();
        }

        private EventCallback<FocusEvent> OnFocus(Color focusColor)
        {
            return _ =>
            {
                Header.style.backgroundColor = focusColor;
                Focussed?.Invoke(this, Node);

                var relativeNodePosition = this.ChangeCoordinatesTo(ParentScrollView, Vector2.zero);
                // use schedule to allow UIToolkit to recalculate. This is a common problem using UIToolkit.
                ParentScrollView.schedule.Execute(() =>
                {
                    if (relativeNodePosition.y <= UI_NODE_HEADER_HEIGHT)
                    {
                        ParentScrollView.scrollOffset = new Vector2(
                            0,
                            Mathf.Clamp(
                                ParentScrollView.scrollOffset.y - UI_NODE_HEADER_HEIGHT,
                                0f,
                                float.MaxValue));
                    }
                    else if (relativeNodePosition.y >= ParentScrollView.layout.height - UI_NODE_HEADER_HEIGHT)
                    {
                        ParentScrollView.scrollOffset += new Vector2(0, UI_NODE_HEADER_HEIGHT);
                    }
                }).StartingIn(100);
            };
        }

        private void Collapse()
        {
            if (IsLocked)
                return;
            _contentContainer?.Clear();
            foreach (var node in ChildUiNodes)
                node.Destroy();
            ChildUiNodes.Clear();
        }

        protected virtual void Destroy()
        {
        }

        private void Expand()
        {
            foreach (var childNode in Node.Children) // add pooling I suppose 
                AddChild(childNode);
        }

        private void AddChild(Node node)
        {
            var uiNode = CreateNode(node);
            if (uiNode == null)
                return;

            uiNode.RegisterScrollView(ParentScrollView);
            ChildUiNodes.Add(uiNode);
            _contentContainer.Add(uiNode);
        }

        protected abstract BaseUiNode CreateNode(Node node);

        private void SetLabelText(bool isExpanded)
        {
            Label.text = $"{(isExpanded ? "▼" : "►")} {Node.Name}";
        }

        private VisualElement BuildContentContainer()
        {
            var container = new VisualElement
            {
                name = "content-container",
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column),
                    flexGrow = 1
                }
            };
            Add(container);
            ChildUiNodes = new();
            return container;
        }

        internal void FocusOn(Stack<Node> nodes)
        {
            if (nodes.TryPop(out var child))
            {
                if (!_isExpanded)
                {
                    Expand();
                    _isExpanded = true;
                }

                foreach (var uiNode in ChildUiNodes)
                {
                    if (uiNode.Node != child)
                        continue;

                    uiNode.FocusOn(nodes);
                    return;
                }

                return;
            }

            Focus();
        }

        internal void RegisterScrollView(ScrollView parentScrollView)
        {
            ParentScrollView = parentScrollView;
            if (Node.IsLeaf || ChildUiNodes == null)
                return;
            foreach (var uiNode in ChildUiNodes)
                uiNode.RegisterScrollView(ParentScrollView);
        }

        private void SetIsLockedPropagating(bool isLocked)
        {
            if (isLocked)
                _lockedCounter++;
            else
                _lockedCounter--;

            ParentUiNode?.SetIsLockedPropagating(isLocked);
        }

        internal void RegisterDrop(SelectableNode selectableNode)
        {
            SetIsLockedPropagating(true);
            ChildUiNodes.Add(selectableNode);
            selectableNode.RegisterScrollView(ParentScrollView);
            if (!IsExpanded)
                ExpandOrCollapse();
        }

        internal void UnRegisterDrop()
        {
            SetIsLockedPropagating(false);
            ParentUiNode.ChildUiNodes.Remove(this);
        }
    }
}