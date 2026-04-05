using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using HamerSoft.Victoria.Core.Extractor.Nodes;

namespace HamerSoft.Victoria.Core.Extractor
{
    public static class Extractor
    {
        private const string COM = "com.";

        public static Folder Parse(FileInfo inputFile)
        {
            using MemoryStream tarStream = new MemoryStream();
            using (var inStream = File.OpenRead(inputFile.FullName))
            using (var gzip = new GZipStream(inStream, CompressionMode.Decompress))
            {
                gzip.CopyTo(tarStream);
            }

            tarStream.Position = 0;

            ExtractTar(inputFile.Name, tarStream, out var assets);
            return assets;
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