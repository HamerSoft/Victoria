using System.Collections.Generic;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Core.Import;
using HamerSoft.Victoria.Ui.SleurEnPleur;

namespace HamerSoft.Victoria.Ui.Elements.Nodes
{
    internal class UiNodeFactory
    {
        private static List<SelectableNode> _nodes;
        private static ImportManifest _importManifest;
        private static IDragParent _dragParent;

        public UiNodeFactory(ImportManifest importManifest, IDragParent dragParent)
        {
            _importManifest = importManifest;
            _dragParent = dragParent;
            _nodes = new List<SelectableNode>(100);
        }

        public void Destroy()
        {
            _importManifest = null;
            _dragParent = null;
            foreach (var selectableNode in _nodes)
            {
                selectableNode.Destroyed -= NodeDestroyed;
                selectableNode.Dropped -= NodeDropped;
                selectableNode.DragStarted -= NodeDragStarted;
            }

            _nodes.Clear();
        }

        internal static SelectableNode CreateSelectableNode(Node node, int depth, SelectableNode parentNode)
        {
            var uiNode = _importManifest.Contains(node)
                ? null
                : new SelectableNode(node, depth, parentNode);

            if (uiNode == null)
                return null;
            _nodes.Add(uiNode);
            uiNode.Destroyed += NodeDestroyed;
            uiNode.Dropped += NodeDropped;
            uiNode.DragStarted += NodeDragStarted;
            return uiNode;
        }

        private static void NodeDragStarted(SelectableNode node)
        {
            node.RemoveFromHierarchy();
            _dragParent.Add(node);
            node.BringToFront();
        }

        private static void NodeDropped(SelectableNode node, BaseUiNode parent)
        {
            if (parent != null)
            {
                _importManifest.Remove(node);
                node.Reparent(parent);
                _importManifest.Add(node, parent);
            }
            else
            {
                node.StopDrag();
                node.MarkAsDirty();
            }
        }

        private static void NodeDestroyed(SelectableNode baseUiNode)
        {
            baseUiNode.Destroyed -= NodeDestroyed;
            baseUiNode.Dropped -= NodeDropped;
            baseUiNode.DragStarted -= NodeDragStarted;
            _nodes.Remove(baseUiNode);
        }

        internal static DestinationUiNode CreateDestinationUiNode(Node node, int depth, DestinationUiNode parentNode)
        {
            return node.IsLeaf
                ? null
                : new DestinationUiNode(node, depth, parentNode);
        }
    }
}