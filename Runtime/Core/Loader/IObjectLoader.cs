using System.Threading;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Extractor;

namespace HamerSoft.Victoria.Loader.Loader
{
    public interface IObjectLoader
    {
        public Task<T> LoadObject<T>(string id, byte[] data, Extractor.Asset.Preview type, CancellationToken token);
    }
}