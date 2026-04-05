using System.Threading;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Extractor;
using HamerSoft.Victoria.Core.Extractor.Nodes;

namespace HamerSoft.Victoria.Loader.Loader
{
    public interface IObjectLoader
    {
        public Task<T> LoadObject<T>(string id, byte[] data, Asset.Preview type, CancellationToken token);
    }
}