using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Ui.Elements.Nodes;
using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui.Elements
{
    // copy of the file tree from the project perspective. 
    // Can traverse path only down, not up (security)
    // drag and drop from importer to destination and flag elements to be selected
    // import folders wherever you want VICTORIA
    public class DestinationElement : VisualElement
    {
        private Node _node;
        private readonly UnityPackage _unityPackage;
        private VisualElement _container;
        private Label _destinationLabel;
        private string _destination;
        private FileSystemNode _destinationNode;
        private DestinationUiNode _rootNode;
        private ScrollView _scrollView;
        private readonly string _rootImportPath;
        private Label _importLabel;
        private Action _onFinishedImport;

        public DestinationElement(UnityPackage unityPackage, Action onFinishedImport)
        {
            _onFinishedImport = onFinishedImport;
            _unityPackage = unityPackage;
            name = "destination";
            _rootImportPath = Application.isEditor
                ? Path.Combine(Application.dataPath, "..")
                : Application.dataPath;
#pragma warning disable CS4014
            ShowLoading();
#pragma warning restore CS4014
        }

        private async Task ShowLoading()
        {
            Add(new Label("Destination loading...")
            {
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            });

            _destinationNode = await Task.Run(() => LoadFileSystemNode(_rootImportPath));
            Clear();
            var header = new VisualElement
            {
                name = "destination_header",
                style =
                {
                    height = 20,
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.RowReverse),
                    marginBottom = 5,
                    marginLeft = 5,
                    marginRight = 5,
                    marginTop = 5
                }
            };
            header.Add(new Button(Import)
            {
                name = "import-button",
                text = "Import",
                style =
                {
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            });
            header.Add(_importLabel = new Label
            {
                name = "import_label",
                style =
                {
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            });

            Add(header);
            _scrollView = new ScrollView
            {
                style =
                {
                    flexGrow = 1
                },
                name = "destination_view"
            };
            Add(_rootNode = new DestinationUiNode(_destinationNode, 0, null));
            _rootNode.RegisterScrollView(_scrollView);
            _scrollView.contentContainer.Add(_rootNode);
            _rootNode.ExpandOrCollapse();
            Add(_scrollView);
        }

        private async void Import()
        {
            try
            {
                if (ImportManifest.Imports.Count == 0)
                {
                    Debug.Log("Nothing to import...");
                    return;
                }

                Debug.Log("importing.");
                var trueImports = ImportManifest.Imports;
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
                    if (uniqueWrites.Contains(node))
                        continue;
                    _importLabel.text = $"Importing: {index} / {totalNodes}";
                    await node.WriteOut(_rootImportPath);
                    uniqueWrites.Add(node);
                    index++;
                }

                uniqueWrites = null;
                _onFinishedImport?.Invoke();
                return;

                List<Node> CollectNodesToWriteOut(SelectableNode rootNoteToWrite)
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
            }
            catch (Exception e)
            {
                Debug.LogError($"Import failed: {e}");
            }
        }

        private FileSystemNode LoadFileSystemNode(string destination)
        {
            try
            {
                var destinationDir = new DirectoryInfo(destination);
                return destinationDir.Exists
                    ? new FileSystemNode(destinationDir)
                    : null;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
    }
}