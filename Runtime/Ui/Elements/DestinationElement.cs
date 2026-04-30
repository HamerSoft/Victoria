using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Core.Import;
using HamerSoft.Victoria.Ui.Elements.Nodes;
using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui.Elements
{
    // copy of the file tree from the project perspective. 
    // Can traverse path only down, not up (security)
    // drag and drop from importer to destination and flag elements to be selected
    // import folders wherever you want VICTORIA
    internal class DestinationElement : VisualElement
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
        private readonly Action _onFinishedImport;
        private ImportManifest _importManifest;

        public DestinationElement(UnityPackage unityPackage, ImportManifest importManifest, Action onFinishedImport)
        {
            _importManifest = importManifest;
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
                await _unityPackage.Import(_rootImportPath, _importManifest,
                    importLabelText => { _importLabel.text = importLabelText; });
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