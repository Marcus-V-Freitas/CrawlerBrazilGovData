namespace SPGovernmentData.Domain.Entities
{
    public class DataSource
    {
        public int Id { get; private set; }
        public string Title { get; private set; }
        public string Url { get; private set; }
        public string DownloadedFolder { get; private set; }
        public int? AditionalInformationId { get; private set; }
        public int? DatasetId { get; private set; }

        public virtual Dataset Dataset { get; private set; }

        public virtual DataSourceAditionalInformation AditionalInformation { get; private set; }

        public DataSource()
        {
        }

        public DataSource(string title, string url, string downloadedFolder, int? aditionalInformationId, int? datasetId)
        {
            Title = title;
            Url = url;
            DownloadedFolder = downloadedFolder;
            AditionalInformationId = aditionalInformationId;
            DatasetId = datasetId;
        }
    }
}