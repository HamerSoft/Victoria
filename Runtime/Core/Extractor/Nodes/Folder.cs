using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HamerSoft.Victoria.Core.Extractor.Nodes
{
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
        public override bool IsLeaf => false;
        public override string DetailedName => Name;
        public override string FullPath => Path;

        public Folder(string name)
        {
            Children = new(EqualityComparer);
            Name = name;
            Path = Name;
        }

        public bool TryGetChild(string directoryName, out Folder child)
        {
            child = null;
            StubNode.Name = directoryName;
            Children.TryGetValue(StubNode, out var childNode);
            if (childNode is Folder folder)
            {
                child = folder;
            }

            return child != null;
        }

        public void AddChild(Node node)
        {
            Children.Add(node);
            node.Parent = this;
        }

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