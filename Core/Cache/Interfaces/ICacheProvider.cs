using System.Threading.Tasks;

namespace Core.Cache.Interfaces
{
    public interface ICacheProvider
    {
        Task<string> GetOrCreateAsync(object key, string value = null);
    }
}