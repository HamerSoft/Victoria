using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HamerSoft.Victoria.Core.Extractor.Nodes
{
    public sealed class FileSystemNode : Node
    {
        private readonly DirectoryInfo _selfDirectory;
        private readonly FileInfo _selfFile;
        public override bool IsLeaf { get; }
        public override string DetailedName => IsLeaf ? $"{Name}{FileExtension()}" : Name;
        public override string FullPath => IsLeaf ? _selfFile.FullName : _selfDirectory.FullName;

        public FileSystemNode(DirectoryInfo directoryInfo)
        {
            _selfDirectory = directoryInfo;
            IsLeaf = false;
            Name = directoryInfo.Name;
            Path = directoryInfo.FullName;

            var subDirectories = _selfDirectory.GetDirectories();
            var files = _selfDirectory.GetFiles();

            Children = new HashSet<Node>(subDirectories.Length + files.Length);
            foreach (var file in files)
                AddChild(new FileSystemNode(file));
            foreach (var subDirectory in subDirectories)
                AddChild(new FileSystemNode(subDirectory));
        }

        private void AddChild(Node node)
        {
            Children.Add(node);
            node.Parent = this;
        }

        protected override void ResolvePath()
        {
            // do nothing, path is already correct
        }

        public override Task WriteOut(string _)
        {
            throw new NotSupportedException("File system node already exists!");
        }

        private FileSystemNode(FileInfo fileInfo)
        {
            Name = fileInfo.Name;
            Path = fileInfo.FullName;
            _selfFile = fileInfo;
            IsLeaf = true;
            Children = new HashSet<Node>();
        }

        public string FileExtension()
        {
            return IsLeaf
                ? _selfFile.Extension
                : "";
        }
    }
}