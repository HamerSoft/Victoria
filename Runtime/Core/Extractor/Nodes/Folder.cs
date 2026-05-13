using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HamerSoft.Victoria.Core.Extractor.Nodes
{
    /// <summary>
    /// Class representing a folder of the imported .unityPackage
    /// </summary>
    public sealed class Folder : Node
    {
        private class NodeEqualityComparer : IEqualityComparer<Node>
        {
            public bool Equals(Node x, Node y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Name == y.Name;
            }

            public int GetHashCode(Node obj)
            {
                return HashCode.Combine(obj.Name);
            }
        }

        private static readonly Node StubNode = new Folder("");
        private static readonly NodeEqualityComparer EqualityComparer = new NodeEqualityComparer();

        /// <summary>
        /// A folder is never a leaf, it can have children but does not have to
        /// </summary>
        public override bool IsLeaf => false;
        /// <summary>
        /// Folder name
        /// </summary>
        public override string DetailedName => Name;

        public Folder(string name)
        {
            Children = new(EqualityComparer);
            Name = name;
            Path = Name;
        }

        /// <summary>
        /// Try to find a direct child folder by name
        /// </summary>
        /// <param name="folderName">Name of the child folder to find</param>
        /// <param name="child">the child folder, or null</param>
        /// <returns>true if the direct child exists</returns>
        public bool TryGetChildFolder(string folderName, out Folder child)
        {
            child = null;
            StubNode.Name = folderName;
            Children.TryGetValue(StubNode, out var childNode);
            if (childNode is Folder folder)
            {
                child = folder;
            }

            return child != null;
        }

        /// <summary>
        /// Add a new child Node to this folder
        /// </summary>
        /// <param name="node">reference to the node to add</param>
        public void AddChild(Node node)
        {
            Children.Add(node);
            node.Parent = this;
        }

        /// <inheritdoc/>
        public override Task WriteOut(string rootPath)
        {
            try
            {
                var path = MergeOverlappingPaths(Path, rootPath);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}