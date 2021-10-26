namespace Core.Utils
{
    public abstract class GovDataUtils
    {
        protected readonly string _baseUrlGov = "http://dados.prefeitura.sp.gov.br";
        protected readonly string _searchDatasets = "pt_PT/dataset?q={0}&sort=score+desc%2C+metadata_modified+desc";
    }
}