using System;
using System.Collections.Generic;
using System.Linq;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Ui.SleurEnPleur;
using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui.Elements.Nodes
{
    internal class SelectableNode : BaseUiNode
    {
        private readonly Toggle _toggle;
        private bool _isSelected;
        private readonly SelectableNode _originalParentUiNode;
        private readonly VisualElement _originalContentContainer;
        private int _originalIndex;
        public bool IsSelected => _isSelected;
        public IEnumerable<SelectableNode> ChildrenNodes => base.ChildUiNodes.Cast<SelectableNode>();

        public event Action<SelectableNode> Destroyed;
        public event Action<SelectableNode> DragStarted;
        public event Action<SelectableNode, BaseUiNode> Dropped;

        public SelectableNode(Node node, int depth, VisualElement parent) : this(node, depth, null)
        {
            _originalContentContainer = parent;
        }

        public SelectableNode(Node node, int depth, SelectableNode parentNode) : base(node, depth, parentNode)
        {
            _originalContentContainer = parentNode?.contentContainer;
            _isSelected = parentNode?._isSelected ?? true;
            Header.Insert(0, _toggle = new Toggle());

            _toggle.SetValueWithoutNotify(_isSelected);
            _toggle.RegisterValueChangedCallback(evt =>
            {
                _isSelected = evt.newValue;
                SelectChildren(_isSelected);
                if (_isSelected)
                    parentNode?.PropagateSelected(true);
                evt.StopPropagation();
                Focus();
            });

            RegisterCallback<KeyDownEvent>(keyDownEvent =>
            {
                if (focusController.focusedElement != this)
                    return;

                switch (keyDownEvent.keyCode)
                {
                    case KeyCode.Return:
                        SetSelectedRecursive(!_isSelected);
                        break;
                    default:
                        return;
                }

                keyDownEvent.StopPropagation();
            });

            this.AddManipulator(new DragAndDropManipulator(this));
        }

        private void SelectChildren(bool isSelected)
        {
            if (ChildUiNodes == null)
                return;

            foreach (var node in ChildUiNodes)
                if (node is SelectableNode uiNode)
                    uiNode.SetSelectedRecursive(isSelected);
        }

        private void SetSelectedRecursive(bool isSelected)
        {
            _isSelected = isSelected;
            _toggle.SetValueWithoutNotify(_isSelected);
            SelectChildren(_isSelected);
        }

        private void PropagateSelected(bool isSelected)
        {
            _toggle.SetValueWithoutNotify(isSelected);
            (ParentUiNode as SelectableNode)?.PropagateSelected(isSelected);
        }

        protected override BaseUiNode CreateNode(Node node)
        {
            return UiNodeFactory.CreateSelectableNode(node, Depth + 1, this);
        }

        internal void Reparent(BaseUiNode destinationUiNode)
        {
            RemoveFromHierarchy();
            destinationUiNode.Add(this);
            Depth = destinationUiNode.OriginalDepth + 1;
            style.marginLeft = Depth * DepthMultiplier;
            UnRegisterDrop();
            destinationUiNode.RegisterDrop(this);
            Node.Parent.Children.Remove(Node);
            Node.Parent = destinationUiNode.Node;
            ParentUiNode = destinationUiNode;
            MarkAsDirty();
        }

        protected override void Destroy()
        {
            Destroyed?.Invoke(this);
            base.Destroy();
        }

        public void MarkAsDirty()
        {
            style.left = 0;
            style.top = 0;
            MarkDirtyRepaint();
        }

        internal void StartDrag()
        {
            _originalIndex = _originalContentContainer.IndexOf(this);
            DragStarted?.Invoke(this);
        }

        internal void StopDrag()
        {
            RemoveFromHierarchy();
            _originalContentContainer.Insert(_originalIndex, this);
        }

        internal void Drop(BaseUiNode dropTarget)
        {
            Dropped?.Invoke(this, dropTarget);
        }
    }
}