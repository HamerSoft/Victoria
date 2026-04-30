using System.IO;
using System.Threading.Tasks;

namespace HamerSoft.Victoria.Core.Extractor.Nodes
{
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
}