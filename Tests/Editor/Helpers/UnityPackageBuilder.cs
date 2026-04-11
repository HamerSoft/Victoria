using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace HamerSoft.Victoria.Tests.Editor.Helpers
{
    /// <summary>
    /// Builds a synthetic .unitypackage (gzipped tar) in a temp file for use in
    /// Extractor tests. Each AddAsset call adds one asset entry to the archive.
    /// Caller is responsible for deleting the file returned by Build().
    /// </summary>
    internal class UnityPackageBuilder
    {
        private struct AssetEntry
        {
            internal string Pathname;
            internal byte[] Content;
            internal string Meta;
            internal byte[] PreviewContent;
            internal string PreviewExtension;
        }

        private readonly string _filename;
        private readonly List<AssetEntry> _assets = new();

        internal UnityPackageBuilder(string filename = "test.unitypackage")
        {
            _filename = filename;
        }

        /// <param name="pathname">Asset path as it would appear in Unity, e.g. "Scripts/MyScript.cs"</param>
        /// <param name="content">Raw file bytes written to the 'asset' entry</param>
        /// <param name="meta">Text written to the 'asset.meta' entry</param>
        /// <param name="previewContent">Raw bytes written to the preview entry</param>
        /// <param name="previewExtension">Extension for the preview file, e.g. ".png"</param>
        internal UnityPackageBuilder AddAsset(
            string pathname,
            byte[] content = null,
            string meta = null,
            byte[] previewContent = null,
            string previewExtension = null)
        {
            _assets.Add(new AssetEntry
            {
                Pathname = pathname,
                Content = content,
                Meta = meta,
                PreviewContent = previewContent,
                PreviewExtension = previewExtension
            });
            return this;
        }

        /// <summary>
        /// Writes the package to a temp file and returns a FileInfo pointing to it.
        /// </summary>
        internal FileInfo Build()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), _filename);

            using (var fileStream = File.Create(tempPath))
            using (var gzip = new GZipStream(fileStream, CompressionMode.Compress))
            {
                foreach (var entry in _assets)
                {
                    var guid = Guid.NewGuid().ToString("N");

                    // Directory entry — signals start of a new asset in the Extractor
                    WriteHeader(gzip, guid + "/", 0, '5');

                    // Pathname file — tells the Extractor the asset's project path
                    var pathBytes = Encoding.UTF8.GetBytes(entry.Pathname + "\n");
                    WriteHeader(gzip, guid + "/pathname", pathBytes.Length, '0');
                    WriteData(gzip, pathBytes);

                    // Asset binary content
                    if (entry.Content != null)
                    {
                        WriteHeader(gzip, guid + "/asset", entry.Content.Length, '0');
                        WriteData(gzip, entry.Content);
                    }

                    // Meta file
                    if (entry.Meta != null)
                    {
                        var metaBytes = Encoding.UTF8.GetBytes(entry.Meta);
                        WriteHeader(gzip, guid + "/asset.meta", metaBytes.Length, '0');
                        WriteData(gzip, metaBytes);
                    }

                    // Preview image/model
                    if (entry.PreviewContent != null && entry.PreviewExtension != null)
                    {
                        WriteHeader(gzip, guid + "/preview" + entry.PreviewExtension,
                            entry.PreviewContent.Length, '0');
                        WriteData(gzip, entry.PreviewContent);
                    }
                }

                // Terminating zero block — Extractor breaks on this
                gzip.Write(new byte[512], 0, 512);
            }

            return new FileInfo(tempPath);
        }

        // The Extractor only reads three fields from the 512-byte header:
        //   name      : bytes   0-99  (ASCII string)
        //   size      : bytes 124-135 (11-digit octal + null)
        //   typeflag  : byte  156     ('0' file, '5' directory)
        // Everything else can remain zero.
        private static void WriteHeader(Stream stream, string name, long size, char typeFlag)
        {
            var header = new byte[512];

            var nameBytes = Encoding.ASCII.GetBytes(name);
            Array.Copy(nameBytes, 0, header, 0, Math.Min(nameBytes.Length, 100));

            var sizeStr = Convert.ToString(size, 8).PadLeft(11, '0');
            var sizeBytes = Encoding.ASCII.GetBytes(sizeStr);
            Array.Copy(sizeBytes, 0, header, 124, sizeBytes.Length); // byte 135 stays 0

            header[156] = (byte)typeFlag;

            stream.Write(header, 0, 512);
        }

        private static void WriteData(Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
            var padding = (512 - (data.Length % 512)) % 512;
            if (padding > 0)
                stream.Write(new byte[padding], 0, padding);
        }
    }
}
