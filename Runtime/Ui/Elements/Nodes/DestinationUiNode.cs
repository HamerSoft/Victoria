using HamerSoft.Victoria.Core.Extractor.Nodes;

namespace HamerSoft.Victoria.Ui.Elements.Nodes
{
    public class DestinationUiNode : BaseUiNode
    {
        protected override int DepthMultiplier => 4;

        public DestinationUiNode(Node node, int depth, BaseUiNode parentNode) : base(node, depth, parentNode)
        {
            if (node.IsLeaf)
                Label.SetEnabled(false);
        }

        protected override BaseUiNode CreateNode(Node node)
        {
            return node.IsLeaf ? null : new DestinationUiNode(node, Depth + 1, this);
        }
    }
}