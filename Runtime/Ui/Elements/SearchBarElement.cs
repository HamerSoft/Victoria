using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HamerSoft.Victoria.Core.Extractor;
using HamerSoft.Victoria.Core.Extractor.Nodes;
using HamerSoft.Victoria.Core.Search;
using HamerSoft.Victoria.Ui.SleurEnPleur;
using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui.Elements
{
    public class SearchBarElement : VisualElement
    {
        private class SearchResults : VisualElement
        {
            private readonly IList _matches;
            private readonly Action<Node> _selected;
            private readonly Action _close;

            private class SearchLabel : VisualElement
            {
                private readonly Label _label;
                private Node Node { get; set; }

                public SearchLabel()
                {
                    Add(_label = new Label()
                    {
                        style =
                        {
                            unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft)
                        }
                    });
                    RegisterCallback<PointerMoveEvent>(evt => { evt.StopPropagation(); });
                }

                public void SetContent(Node node)
                {
                    Node = node;
                    _label.text = Node.Name;
                }
            }

            public SearchResults(IList matches, Vector2 position, Action close, Action<Node> selected)
            {
                _close = close;
                _selected = selected;
                _matches = matches;
                style.left = position.x;
                style.top = position.y;
                style.position = new StyleEnum<Position>(Position.Absolute);
                style.minHeight = 40;
                style.maxHeight = 300;
                style.width = 200;
                style.backgroundColor = new Color(0.37f, 0.37f, 0.37f, 1);
                style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
                style.borderLeftWidth = 1;
                style.borderRightWidth = 1;
                style.borderTopWidth = 1;
                style.borderBottomWidth = 1;
                style.borderLeftColor = Color.black;
                style.borderRightColor = Color.black;
                style.borderTopColor = Color.black;
                style.borderBottomColor = Color.black;
                style.transformOrigin = new TransformOrigin(0, 0, 0);
                usageHints = UsageHints.DynamicTransform;
                var header = new VisualElement
                {
                    style =
                    {
                        flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                        height = 20,
                        backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.4f)
                    },
                    pickingMode = PickingMode.Ignore
                };

                header.Add(new Label("Search Results:")
                {
                    style =
                    {
                        unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft),
                        unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold),
                        flexGrow = 1
                    },
                    pickingMode = PickingMode.Ignore
                });
                header.Add(new Button(_close)
                {
                    text = "X",
                    style =
                    {
                        unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter),
                        width = 20,
                        height = 20
                    }
                });
                Add(header);
                this.AddManipulator(new DragManipulator(this));

                var listView = new ListView(matches, -1, MakeItem, BindItem)
                {
                    selectionType = SelectionType.Single,
                    style =
                    {
                        flexGrow = 1,
                        marginLeft = 2,
                        marginRight = 2,
                        marginTop = 2,
                        marginBottom = 2,
                    }
                };
                listView.onSelectionChange += ListViewOnSelectionChange;
                Add(listView);
            }

            private void ListViewOnSelectionChange(IEnumerable<object> selection)
            {
                var selected = selection.First() as Node;
                Debug.LogWarning($"Selected = {selected.Name}");
                _selected?.Invoke(selected);
                _close?.Invoke();
            }

            private void BindItem(VisualElement element, int matchIndex)
            {
                if (element is not SearchLabel searchLabel)
                {
                    Debug.LogError("Failed to bind search match to element");
                    return;
                }

                searchLabel.SetContent(_matches[matchIndex] as Node);
            }

            private VisualElement MakeItem()
            {
                return new SearchLabel();
            }
        }

        private readonly TextField _searchField;
        private readonly Button _advancedSearch;
        private readonly VisualElement _popupParent;
        private readonly ISearch _search;
        private readonly Action<Node> _selected;
        private string _currentSearchTerm;
        private SearchResults _searchResultsPopup;

        public SearchBarElement(ISearch search, VisualElement popupParent, Action<Node> selected)
        {
            _selected = selected;
            _popupParent = popupParent;
            _search = search;
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            style.height = 20;
            style.marginRight = 5;
            style.marginLeft = 5;
            style.marginTop = 5;
            style.marginBottom = 5;

            Add(new Label("Search: ")
            {
                name = "searchbar-label",
                style =
                {
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft),
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                }
            });

            Add(_searchField = new TextField()
            {
                style =
                {
                    flexGrow = 1
                }
            });
            _searchField.RegisterValueChangedCallback(SearchStringChangedCallback);
            _searchField.RegisterCallback<KeyDownEvent>(OnSubmitCallback);
        }

        private void OnSubmitCallback(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Return)
                return;

            var matches = _search.SearchByName(_currentSearchTerm);
            ClosePopup();
            ShowSearchResults(new List<Node>(matches));
        }

        private void ShowSearchResults(IList matches)
        {
            _searchResultsPopup = new SearchResults(matches, new Vector2(100, 100), ClearSearchResults, _selected);
            _popupParent.Add(_searchResultsPopup);
            _searchResultsPopup.BringToFront();
        }

        private void ClosePopup()
        {
            if (_searchResultsPopup != null)
                _popupParent.Remove(_searchResultsPopup);
            _searchResultsPopup = null;
        }

        private void ClearSearchResults()
        {
            ClosePopup();
            _searchField.SetValueWithoutNotify("");
            _currentSearchTerm = "";
        }

        private void SearchStringChangedCallback(ChangeEvent<string> evt)
        {
            _currentSearchTerm = evt.newValue;
        }
    }
}