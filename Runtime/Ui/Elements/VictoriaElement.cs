using System;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Core.Import;
using HamerSoft.Victoria.Ui.Elements.Nodes;
using HamerSoft.Victoria.Ui.SleurEnPleur;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui.Elements
{
    // add meta data
    public class VictoriaElement : VisualElement, IDragParent
    {
        private VisualElement _header;
        private DetailsElement _detailsView;
        private VisualElement _packageOverView;
        private readonly UiNodeFactory _uiNodeFactory;
        private readonly ImportManifest _importManifest;

        public VictoriaElement(UnityPackage unityPackage, Action onFinishedImport)
        {
            name = "whatever";
            _importManifest = new ImportManifest();
            _uiNodeFactory = new UiNodeFactory(_importManifest, this);
            BaseUiNode.Focussed += NodeFocussed;
            var splitView = new TwoPaneSplitView(1, 200, TwoPaneSplitViewOrientation.Horizontal);
            CreatePackageOverView(splitView, unityPackage);
            CreateDetailsView(splitView, unityPackage, onFinishedImport);
            Add(splitView);
        }

        internal void Destroy()
        {
            BaseUiNode.Focussed -= NodeFocussed;
            _importManifest.Destroy();
            _uiNodeFactory.Destroy();
        }

        private void NodeFocussed(BaseUiNode uiNode, Node node)
        {
            if (uiNode is not SelectableNode)
                return;

            _detailsView.SetNode(node);
        }

        private void CreateDetailsView(TwoPaneSplitView splitView, UnityPackage unityPackage, Action onFinishedImport)
        {
            splitView.Add(_detailsView = new DetailsElement(unityPackage, _importManifest, onFinishedImport));
        }

        private void CreatePackageOverView(TwoPaneSplitView splitView, UnityPackage unityPackage)
        {
            splitView.Add(_packageOverView = new PackageOverview(unityPackage));
        }

        public void Add(BaseUiNode uiNode)
        {
            Add((VisualElement)uiNode);
        }
    }
}