using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HamerSoft.Victoria.Core.Extractor.Nodes
{
    /// <summary>
    /// A <see cref="Node"/> implementation that wraps an existing file-system entry.
    /// Constructed from a <see cref="DirectoryInfo"/> (non-leaf) or a <see cref="FileInfo"/> (leaf).
    /// Unlike package-extracted nodes, file-system nodes already exist on disk and cannot be written out.
    /// </summary>
    public sealed class FileSystemNode : Node
    {
        private readonly DirectoryInfo _selfDirectory;
        private readonly FileInfo _selfFile;
        /// <summary>
        /// <c>true</c> if this node represents a file; <c>false</c> if it represents a directory.
        /// </summary>
        public override bool IsLeaf { get; }
        /// <summary>
        /// The display name of this node. For files this includes the file extension (e.g. <c>icon.png</c>);
        /// for directories it is the bare folder name.
        /// </summary>
        public override string DetailedName => IsLeaf ? $"{Name}{FileExtension()}" : Name;

        /// <summary>
        /// Creates a non-leaf node representing a directory. Recursively builds child
        /// <see cref="FileSystemNode"/> instances for all files and subdirectories it contains.
        /// </summary>
        /// <param name="directoryInfo">The directory to represent.</param>
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

        /// <summary>
        /// Not supported for file-system nodes — the entry already exists on disk.
        /// </summary>
        /// <param name="_">Unused root path parameter.</param>
        /// <returns>Never returns; always throws <see cref="NotSupportedException"/>.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
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

        /// <summary>
        /// Gets the file extension of this node.
        /// </summary>
        /// <returns>
        /// The extension including the leading dot (e.g. <c>.png</c>) for leaf nodes,
        /// or an empty string for directory nodes.
        /// </returns>
        public string FileExtension()
        {
            return IsLeaf
                ? _selfFile.Extension
                : "";
        }
    }
}