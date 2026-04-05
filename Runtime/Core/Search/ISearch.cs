using System.Collections.Generic;

namespace HamerSoft.Victoria.Core.Search
{
    public interface ISearch
    {
        IEnumerable<Extractor.Extractor.Node> SearchByName(string currentSearchTerm);
    }
}