using System.Collections.Generic;
using HamerSoft.Victoria.Core.Extractor.Nodes;

namespace HamerSoft.Victoria.Core.Search
{
    public interface ISearch
    {
        IEnumerable<Node> SearchByName(string currentSearchTerm);
    }
}