using System;
using System.Collections.Generic;
using HamerSoft.Victoria.Core.Extractor.Nodes;

namespace HamerSoft.Victoria.Core.Search
{
    internal class Searcher : ISearch
    {
        private readonly Folder _root;

        public Searcher(Folder root)
        {
            _root = root;
        }

        // Breadth first search is used
        public IEnumerable<Node> SearchByName(string currentSearchTerm)
        {
            List<Node> matches = new List<Node>(_root.Children.Count);
            Queue<Node> scope = new Queue<Node>(_root.Children);
            while (scope.TryDequeue(out var currentNode))
            {
                if (currentNode.DetailedName.Contains(currentSearchTerm, StringComparison.InvariantCultureIgnoreCase))
                    matches.Add(currentNode);

                if (!currentNode.HasChildren)
                    continue;

                foreach (var child in currentNode.Children)
                    scope.Enqueue(child);
            }

            return matches;
        }
    }
}