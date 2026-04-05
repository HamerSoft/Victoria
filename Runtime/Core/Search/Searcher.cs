using System;
using System.Collections.Generic;

namespace HamerSoft.Victoria.Core.Search
{
    public class Searcher : ISearch
    {
        private readonly Extractor.Extractor.Folder _root;

        public Searcher(Extractor.Extractor.Folder root)
        {
            _root = root;
        }

        // Breadth first search is used
        public IEnumerable<Extractor.Extractor.Node> SearchByName(string currentSearchTerm)
        {
            List<Extractor.Extractor.Node> matches = new List<Extractor.Extractor.Node>(_root.Children.Count);
            Queue<Extractor.Extractor.Node> scope = new Queue<Extractor.Extractor.Node>(_root.Children);
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