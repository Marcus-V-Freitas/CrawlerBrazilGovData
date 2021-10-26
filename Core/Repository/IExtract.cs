using System.Threading.Tasks;

namespace Core.Repository
{
    public interface IExtract
    {
        Task<string> ExtractHTML(string url);
    }
}