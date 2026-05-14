using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HamerSoft.Victoria.Core.Extractor.Nodes
{
    /// <summary>
    /// base class for representing nodes in the file-tree
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// Name of the node
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Full path of the node
        /// </summary>
        public string Path { get; internal set; }
        /// <summary>
        /// Children of the node
        /// </summary>
        /// <remarks>always initialized</remarks>
        public HashSet<Node> Children { get; protected set; }
        /// <summary>
        /// Whether the node is a leaf node
        /// </summary>
        public abstract bool IsLeaf { get; }
        /// <summary>
        /// Full file / folder name
        /// </summary>
        public abstract string DetailedName { get; }

        /// <summary>
        /// The parent node
        /// </summary>
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

        /// <summary>
        /// Check whether the node has children
        /// </summary>
        public bool HasChildren => Children.Count > 0;

        /// <summary>
        /// Write out a node (back) to disk
        /// </summary>
        /// <param name="rootPath">Path where to write it to</param>
        /// <returns>awaitable IO task</returns>
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