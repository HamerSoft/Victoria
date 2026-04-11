using System.Collections.Generic;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Ui.Elements.Nodes;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui.Elements
{
    public class PackageOverview : VisualElement
    {
        private readonly SelectableNode _rootNode;
        private UnityPackage _unityPackage;
        private readonly ScrollView _packageOverView;

        public PackageOverview(UnityPackage unityPackage)
        {
            _unityPackage = unityPackage;
            name = "package-overview";
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
            style.flexGrow = 1;

            var searchBar = new SearchBarElement(unityPackage.Search, this, OnNodeSearched);

            _packageOverView = new ScrollView
            {
                style =
                {
                    flexGrow = 1
                },
                name = "package_overview"
            };

            Add(searchBar);
            Add(_packageOverView);
            _rootNode = new SelectableNode(unityPackage.Assets, 0, (VisualElement)_packageOverView.contentContainer);
            _rootNode.RegisterScrollView(_packageOverView);
            _packageOverView.contentContainer.Add(_rootNode);
            _rootNode.ExpandOrCollapse();
            _rootNode.Focus();
        }

        private void OnNodeSearched(Node searchedNode)
        {
            var parents = new Stack<Node>();

            var currentNode = searchedNode;
            while (currentNode != null)
            {
                parents.Push(currentNode);
                currentNode = currentNode.Parent;
            }

            if (parents.TryPop(out _))
                _rootNode.FocusOn(parents);
        }
    }
}