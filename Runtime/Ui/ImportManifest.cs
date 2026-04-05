using System.Collections.Generic;
using System.Linq;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Ui.Elements.Nodes;

namespace HamerSoft.Victoria.Ui
{
    public static class ImportManifest
    {
        // Child -> Parent
        private static Dictionary<SelectableNode, BaseUiNode> _imports;
        public static IReadOnlyDictionary<SelectableNode, BaseUiNode> Imports => _imports;

        public static void Init()
        {
            _imports = new Dictionary<SelectableNode, BaseUiNode>();
        }

        public static void Destroy()
        {
            _imports.Clear();
        }

        public static void Add(SelectableNode node, BaseUiNode parent)
        {
            _imports[node] = parent;
        }

        public static void Remove(SelectableNode node)
        {
            _imports.Remove(node);
        }

        public static bool Contains(Node node)
        {
           return _imports.Keys.Any(k=> k.Node == node);
        }
    }
}