namespace SPNewsData.Application.Entities.Utils
{
    public abstract class GovDataUtils
    {
        protected readonly string _baseUrlGov = "https://www.saopaulo.sp.gov.br";
        protected readonly string _searchDatasets = "/page/{0}/?s={1}";
    }
}