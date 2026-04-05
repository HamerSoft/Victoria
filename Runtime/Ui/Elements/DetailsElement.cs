using System;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui.Elements
{
    internal class DetailsElement : VisualElement
    {
        private UnityPackage _unityPackage;
        private PreviewElement _previewer;
        private DestinationElement _destination;

        public DetailsElement(UnityPackage unityPackage, Action onFinishedImport)
        {
            _unityPackage = unityPackage;
            var verticalSplit = new TwoPaneSplitView(1, 200, TwoPaneSplitViewOrientation.Vertical);
            CreateDestination(verticalSplit, unityPackage, onFinishedImport);
            CreatePreview(verticalSplit, unityPackage);
            Add(verticalSplit);
        }

        private void CreatePreview(TwoPaneSplitView splitView, UnityPackage unityPackage)
        {
            splitView.Add(_previewer = new PreviewElement(unityPackage));
        }

        private void CreateDestination(TwoPaneSplitView splitView, UnityPackage unityPackage, Action onFinishedImport)
        {
            splitView.Add(_destination = new DestinationElement(unityPackage, onFinishedImport));
        }

        internal void SetNode(Node node)
        {
            _previewer.SetNode(node);
        }
    }
}