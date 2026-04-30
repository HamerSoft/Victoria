using System;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Core.Import;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui.Elements
{
    internal class DetailsElement : VisualElement
    {
        private PreviewElement _previewer;
        private DestinationElement _destination;

        public DetailsElement(UnityPackage unityPackage, ImportManifest importManifest, Action onFinishedImport)
        {
            var verticalSplit = new TwoPaneSplitView(1, 200, TwoPaneSplitViewOrientation.Vertical);
            CreateDestination(verticalSplit, unityPackage, importManifest, onFinishedImport);
            CreatePreview(verticalSplit, unityPackage);
            Add(verticalSplit);
        }

        private void CreatePreview(TwoPaneSplitView splitView, UnityPackage unityPackage)
        {
            splitView.Add(_previewer = new PreviewElement(unityPackage));
        }

        private void CreateDestination(TwoPaneSplitView splitView, UnityPackage unityPackage,
            ImportManifest importManifest, Action onFinishedImport)
        {
            splitView.Add(_destination = new DestinationElement(unityPackage, importManifest, onFinishedImport));
        }

        internal void SetNode(Node node)
        {
            _previewer.SetNode(node);
        }
    }
}