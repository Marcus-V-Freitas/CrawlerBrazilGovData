using System.Threading.Tasks;

namespace Core.Repository
{
    public interface IParser<T>
    {
        Task<T> ParserObject(string html);
    }
}