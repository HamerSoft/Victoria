using System;
using System.IO;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Core.Import;
using HamerSoft.Victoria.Ui.Elements.Nodes;
using HamerSoft.Victoria.Ui.SleurEnPleur;
using UnityEngine;
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

        public VictoriaElement(UnityPackage unityPackage, DirectoryInfo destination, Action onFinishedImport)
        {
            name = "whatever";
            styleSheets.Add(Resources.Load<StyleSheet>("VictoriaEditorDark"));
            style.flexGrow = 1;
            style.backgroundColor = new StyleColor(new Color(0.235f, 0.235f, 0.235f));
            _importManifest = new ImportManifest();
            _uiNodeFactory = new UiNodeFactory(_importManifest, this);
            BaseUiNode.Focussed += NodeFocussed;
            var splitView = new TwoPaneSplitView(1, 200, TwoPaneSplitViewOrientation.Horizontal);
            CreatePackageOverView(splitView, unityPackage);
            CreateDetailsView(splitView, unityPackage, destination, onFinishedImport);
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

        private void CreateDetailsView(TwoPaneSplitView splitView, UnityPackage unityPackage, DirectoryInfo destination,
            Action onFinishedImport)
        {
            splitView.Add(_detailsView =
                new DetailsElement(unityPackage, _importManifest, destination, onFinishedImport));
        }

        private void CreatePackageOverView(TwoPaneSplitView splitView, UnityPackage unityPackage)
        {
            splitView.Add(_packageOverView = new PackageOverview(unityPackage));
        }

        void IDragParent.Add(BaseUiNode uiNode)
        {
            Add((VisualElement)uiNode);
        }
    }
}