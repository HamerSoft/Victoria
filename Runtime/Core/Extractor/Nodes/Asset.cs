using System.IO;
using System.Threading.Tasks;

namespace HamerSoft.Victoria.Core.Extractor.Nodes
{
    /// <summary>
    /// Class representing an asset
    /// </summary>
    public sealed class Asset : Node
    {
        /// <summary>
        /// Type of preview available in this node
        /// </summary>
        public enum Preview
        {
            /// <summary>
            /// Nnot supported
            /// </summary>
            NotSupported = 0,
            /// <summary>
            /// Supported, but no file available
            /// </summary>
            NotAvailable = 1,
            /// <summary>
            /// Plain text e.g. json or c# code
            /// </summary>
            PlainText = 10,
            /// <summary>
            /// Images e.g. materials, sprites, textures, and even 3D models.
            /// </summary>
            Image = 20,
            /// <summary>
            /// Audio preview e.g. mp3
            /// </summary>
            Audio = 30,
        }

        /// <summary>
        /// The identifier of the Asset to use for finding matching files 
        /// </summary>
        public string Identifier { get; internal set; }
        /// <summary>
        /// The content of the binary file
        /// </summary>
        public byte[] FileContent { get; internal set; }
        /// <summary>
        /// The file type (file-extension)
        /// </summary>
        public string ContentType { get; internal set; }
        /// <summary>
        /// The meta-file content as string
        /// </summary>
        public string MetaFile { get; internal set; }
        /// <summary>
        /// The preview content binary data
        /// </summary>
        public byte[] PreviewContent { get; internal set; }
        /// <summary>
        /// The preview file type (file-extension)
        /// </summary>
        public string PreviewType { get; internal set; }
        /// <summary>
        /// Size of the file
        /// </summary>
        public int Size => FileContent.Length;
        /// <summary>
        /// An asset is always a leaf node, it cannot have children (sorry bruh)
        /// </summary>
        public override bool IsLeaf => true;
        /// <summary>
        /// Full file name, with extension
        /// </summary>
        public override string DetailedName => $"{Name}{ContentType}";

        /// <inheritdoc/>
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

        /// <summary>
        /// Get the preview type of the asset
        /// </summary>
        /// <returns>Preview type to show in the UI</returns>
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
                _ => Preview.NotAvailable
            };
        }
    }
}