using System.Collections.Generic;
using HamerSoft.Victoria.Ui.Elements.Nodes;
using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui.SleurEnPleur
{
    public class DragAndDropManipulator : PointerManipulator
    {
        private bool _active;
        private Vector2 _pointerOffsetInTarget;
        private readonly SelectableNode _targetNode;
        private bool _wasDragged;
        private readonly List<VisualElement> _picked;

        public DragAndDropManipulator(SelectableNode target)
        {
            _targetNode = target;
            _picked = new List<VisualElement>();
            this.target = target;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnPointerDown(PointerDownEvent e)
        {
            if (_active)
                return;

            _active = true;
            _wasDragged = false;
            target.CapturePointer(e.pointerId);

            _pointerOffsetInTarget = e.position - (Vector3)target.worldBound.position;
            e.StopImmediatePropagation();
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (!_active || !target.HasPointerCapture(e.pointerId))
                return;

            if (!_wasDragged)
            {
                _targetNode.StartDrag(DragContext.DragParent);
                target.style.position = Position.Absolute;
            }

            _wasDragged = true;
            Vector2 newTopLeftInPanel = e.position - (Vector3)_pointerOffsetInTarget;

            var parent = target.parent ?? target.panel.visualTree;
            Vector2 newTopLeftInParentLocal = parent.WorldToLocal(newTopLeftInPanel);

            target.style.left = newTopLeftInParentLocal.x;
            target.style.top = newTopLeftInParentLocal.y;

            e.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent e)
        {
            if (!_active)
                return;

            if (target.HasPointerCapture(e.pointerId))
                target.ReleasePointer(e.pointerId);

            _active = false;
            target.style.position = Position.Relative;
            target.MarkDirtyRepaint();

            if (!_wasDragged)
            {
                _targetNode?.ExpandOrCollapse();
                _targetNode?.MarkAsDirty();
                return;
            }

            var dropTarget = target.panel.PickAll(e.position, _picked)?.GetFirstAncestorOfType<BaseUiNode>();
            if (dropTarget is not DestinationUiNode)
                foreach (var element in _picked)
                {
                    if (element is not BaseUiNode node)
                        continue;

                    if (node.IsLeaf || node == _targetNode)
                        continue;

                    if (node.ParentScrollView.name.Contains("package_overview"))
                    {
                        Debug.LogWarning("Cannot import asset to source package. Select a destination node.");
                        dropTarget = null;
                        break;
                    }

                    dropTarget = node;
                    break;
                }

            _picked.Clear();

            if (dropTarget != null)
            {
                _targetNode.Reparent(dropTarget);
            }
            else
            {
                _targetNode.StopDrag();
                _targetNode.MarkAsDirty();
            }

            _wasDragged = false;
        }
    }
}