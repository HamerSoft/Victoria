using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui.SleurEnPleur
{
    public class DragManipulator : PointerManipulator
    {
        private bool _active;
        private Vector2 _pointerOffsetInTarget;

        public DragManipulator(VisualElement target)
        {
            this.target = target;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.NoTrickleDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.NoTrickleDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.NoTrickleDown);

            target.style.position = Position.Absolute;
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

            target.CapturePointer(e.pointerId);

            _pointerOffsetInTarget = e.position - (Vector3)target.worldBound.position;

            e.StopImmediatePropagation();
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (!_active || !target.HasPointerCapture(e.pointerId))
                return;

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
            e.StopPropagation();
        }
    }
}