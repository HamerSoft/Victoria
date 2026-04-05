using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Audio;
using HamerSoft.Victoria.Core.Search;
using HamerSoft.Victoria.Loader;

namespace HamerSoft.Victoria.Core.Extractor
{
    public static class Extractor
    {
        private const string COM = "com.";

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

        public sealed class Asset : Node
        {
            public enum Preview
            {
                NotSupported = 0,
                NotAvailable = 1,
                PlainText = 10,
                Image = 20,
                Audio = 30,
                Model = 40
            }

            public string Identifier { get; internal set; }
            public byte[] FileContent { get; internal set; }
            public string ContentType { get; internal set; }
            public string MetaFile { get; internal set; }
            public byte[] PreviewContent { get; internal set; }
            public string PreviewType { get; internal set; }
            public override string FullPath => $"{Path}{ContentType}";
            public int Size => FileContent.Length;
            public override bool IsLeaf => true;
            public override string DetailedName => $"{Name}{ContentType}";

            public override async Task WriteOut(string rootPath)
            {
                var path = MergeOverlappingPaths($"{Path}{ContentType}", rootPath);
                var directory = System.IO.Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                await File.WriteAllBytesAsync(path, FileContent);
            }

            public Asset()
            {
                Children = new();
            }

            public override string ToString()
            {
                return Name;
            }

            public Preview GetPreviewType()
            {
                if (this is
                    {
                        ContentType: ".cs" or ".json" or ".meta" or ".md" or ".txt" or ".uss" or ".uxml" or ".asmdef"
                        or ".asset"
                    })
                {
                    return Preview.PlainText;
                }

                if (this is
                    {
                        ContentType: ".mp3" or ".wav" or ".ogg"
                    })
                {
                    return Preview.Audio;
                }

                return PreviewType?.ToLower() switch
                {
                    ".png" or ".jpg" or ".jpeg" => Preview.Image,
                    ".fbx" or ".dae" or ".3ds" or ".dxf" or ".obj" => Preview.Model,
                    _ => Preview.NotAvailable
                };
            }
        }

        public static UnityPackage Parse(FileInfo inputFile, IAudioSource audioSource)
        {
            using MemoryStream tarStream = new MemoryStream();
            using (var inStream = File.OpenRead(inputFile.FullName))
            using (var gzip = new GZipStream(inStream, CompressionMode.Decompress))
            {
                gzip.CopyTo(tarStream);
            }

            tarStream.Position = 0;

            ExtractTar(inputFile.Name, tarStream, out var assets);
            return new UnityPackage(assets, new ObjectLoader(), new Searcher(assets),
                audioSource);
        }

        static void ExtractTar(string assetName, Stream tarStream, out Folder assets)
        {
            byte[] header = new byte[512];

            const char directoryTypeFlag = '5';
            const string asset = "asset";
            const string preview = "preview";
            const string metaExtension = ".meta";
            const string pathname = "/pathname";

            assets = new Folder(assetName);
            var currentAsset = new Asset();
            while (true)
            {
                int bytesRead = tarStream.Read(header, 0, 512);
                if (bytesRead < 512) break;

                bool allZero = true;
                for (int i = 0; i < 512; i++)
                {
                    if (header[i] != 0)
                    {
                        allZero = false;
                        break;
                    }
                }

                if (allZero) break;

                string name = ReadString(header, 0, 100);
                string sizeString = ReadString(header, 124, 12).Trim();
                long size = Convert.ToInt64(sizeString, 8); // octal
                char typeFlag = (char)header[156];

                string fullPath = name.Replace('/', Path.DirectorySeparatorChar);

                if (typeFlag == directoryTypeFlag)
                {
                    if (!string.IsNullOrWhiteSpace(currentAsset.Identifier))
                        AddAssetToFolder(currentAsset, assets);

                    currentAsset = new Asset
                    {
                        Identifier = name
                    };
                }
                else
                {
                    using var stream = new MemoryStream(new byte[size], true);
                    CopyStream(tarStream, stream, size);
                    if (fullPath.EndsWith(asset))
                    {
                        currentAsset.FileContent = stream.ToArray();
                    }
                    else if (fullPath.EndsWith(metaExtension))
                    {
                        stream.Position = 0;
                        using var s = new StreamReader(stream, Encoding.UTF8);
                        currentAsset.MetaFile = s.ReadToEnd();
                    }
                    else if (Path.GetFileNameWithoutExtension(fullPath) == preview)
                    {
                        currentAsset.PreviewContent = stream.ToArray();
                        currentAsset.PreviewType = Path.GetExtension(fullPath);
                    }
                    else if (name.EndsWith(pathname))
                    {
                        stream.Position = 0;
                        using var s = new StreamReader(stream, Encoding.UTF8);
                        var assetPath = s.ReadLine().Trim();

                        // Set the stream position to the end of the file.        
                        long endPoint = stream.Length;
                        stream.Seek(endPoint, SeekOrigin.Begin);

                        currentAsset.Path = assetPath;
                        currentAsset.Name = Path.GetFileNameWithoutExtension(assetPath);
                        currentAsset.ContentType = Path.GetExtension(assetPath);
                    }

                    long padding = (512 - (size % 512)) % 512;
                    tarStream.Seek(padding, SeekOrigin.Current);
                }
            }

            AddAssetToFolder(currentAsset, assets);
        }

        static string ReadString(byte[] buffer, int offset, int length)
        {
            return Encoding.ASCII.GetString(buffer, offset, length).Trim('\0');
        }

        static void CopyStream(Stream input, Stream output, long bytes)
        {
            byte[] buffer = new byte[8192];
            long remaining = bytes;
            while (remaining > 0)
            {
                int read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
                if (read <= 0) break;
                output.Write(buffer, 0, read);
                remaining -= read;
            }
        }

        private static void AddAssetToFolder(Asset asset, Folder root)
        {
            var assetPath = asset.Path.Split(Path.DirectorySeparatorChar);
            var currentDirectory = root;
            foreach (var part in assetPath)
            {
                if (!part.StartsWith(COM) && !string.IsNullOrWhiteSpace(Path.GetExtension(part)))
                {
                    currentDirectory.AddChild(asset);
                }
                else
                {
                    if (!currentDirectory.TryGetChild(part, out var child))
                    {
                        child = new Folder(part);
                        currentDirectory.AddChild(child);
                    }

                    currentDirectory = child;
                }
            }
        }
    }
}