using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Ui.Elements.Nodes;
using UnityEngine;

namespace HamerSoft.Victoria.Core.Import
{
    internal class Importer : IDisposable
    {
        private string _rootImportPath;
        private ImportManifest _manifest;

        public Importer(string rootImportPath, ImportManifest manifest)
        {
            _manifest = manifest;
            _rootImportPath = rootImportPath;
        }

        internal async Task Import(Action<string> onUpdate)
        {
            try
            {
                if (_manifest.Imports.Count == 0)
                {
                    Debug.Log("Nothing to import...");
                    return;
                }

                Debug.Log("importing.");
                var trueImports = _manifest.Imports;
                List<Node> nodesToWrite = new List<Node>();
                foreach (var import in trueImports)
                {
                    if (!import.Key.IsSelected)
                        continue;
                    var nodes = CollectNodesToWriteOut(import.Key);
                    nodesToWrite.AddRange(nodes);
                }

                int totalNodes = nodesToWrite.Count;
                int index = 0;
                var writeQueue = new Queue<Node>(nodesToWrite);
                var uniqueWrites = new HashSet<Node>();
                while (writeQueue.TryDequeue(out var node))
                {
                    onUpdate?.Invoke($"Importing: {index} / {totalNodes}");
                    if (uniqueWrites.Contains(node))
                        continue;
                    await node.WriteOut(_rootImportPath);
                    uniqueWrites.Add(node);
                    index++;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Import failed: {e}");
                throw;
            }

            onUpdate?.Invoke("Import Complete!");
        }

        internal List<Node> CollectNodesToWriteOut(SelectableNode rootNoteToWrite)
        {
            var selectedNodeQueue = new Queue<SelectableNode>();
            selectedNodeQueue.Enqueue(rootNoteToWrite);
            var nodesToWriteOut = new List<Node>();
            var nonExpandedNodes = new Queue<Node>();
            // collect all expanded and selected assets
            while (selectedNodeQueue.TryDequeue(out var node))
            {
                if (!node.IsSelected)
                    continue;

                nodesToWriteOut.Add(node.Node);
                if (node.IsLeaf)
                    continue;

                if (node.IsExpanded)
                    foreach (var childNode in node.ChildrenNodes)
                        selectedNodeQueue.Enqueue(childNode);
                else
                {
                    foreach (var childNode in node.Node.Children)
                        nonExpandedNodes.Enqueue(childNode);
                }
            }

            while (nonExpandedNodes.TryDequeue(out var node))
            {
                nodesToWriteOut.Add(node);
                if (node.IsLeaf || !node.HasChildren)
                    continue;
                foreach (var child in node.Children)
                    nonExpandedNodes.Enqueue(child);
            }

            return nodesToWriteOut;
        }

        public void Dispose()
        {
            _rootImportPath = null;
            _manifest = null;
        }
    }
}