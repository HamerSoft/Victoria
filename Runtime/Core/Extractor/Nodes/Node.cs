using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HamerSoft.Victoria.Core.Extractor.Nodes
{
    public abstract class Node
    {
        public string Name { get; internal set; }
        public string Path { get; internal set; }
        public HashSet<Node> Children { get; protected set; }
        public abstract bool IsLeaf { get; }
        public abstract string DetailedName { get; }
        public abstract string FullPath { get; }

        public Node Parent
        {
            get => _parent;
            internal set
            {
                _parent = value;
                ResolvePath();
            }
        }

        protected virtual void ResolvePath()
        {
            var path = Name;
            var parent = _parent;
            while (parent != null && !string.IsNullOrWhiteSpace(parent.Name))
            {
                path = System.IO.Path.Combine(parent.Name, path);
                parent = parent._parent;
            }

            Path = path;
            foreach (var child in Children)
                child.Parent = this;
        }

        private Node _parent;

        public bool HasChildren => Children.Count > 0;

        public abstract Task WriteOut(string rootPath);

        protected string MergeOverlappingPaths(string relativePath, string absolutePath)
        {
            string absResolved = System.IO.Path.GetFullPath(absolutePath);
            string firstSegment = relativePath.Split(new[] { '/', System.IO.Path.DirectorySeparatorChar })[0];

            int idx = absResolved.LastIndexOf(System.IO.Path.DirectorySeparatorChar + firstSegment,
                StringComparison.Ordinal);
            string root = absResolved.Substring(0, idx);

            return System.IO.Path.Combine(root, relativePath);
        }
    }
}