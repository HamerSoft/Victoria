using System.Collections.Generic;
using System.Linq;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Ui.Elements.Nodes;

namespace HamerSoft.Victoria.Core.Import
{
    internal sealed class ImportManifest
    {
        // Child -> Parent
        private readonly Dictionary<SelectableNode, BaseUiNode> _imports = new();
        public IReadOnlyDictionary<SelectableNode, BaseUiNode> Imports => _imports;

        public void Destroy()
        {
            _imports.Clear();
        }

        public void Add(SelectableNode node, BaseUiNode parent)
        {
            _imports[node] = parent;
        }

        public void Remove(SelectableNode node)
        {
            _imports.Remove(node);
        }

        public bool Contains(Node node)
        {
            return _imports.Keys.Any(k => k.Node == node);
        }
    }
}