namespace Application.Entities.DTOs
{
    public class DataSourceDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string DownloadedFolder { get; set; }
        public int? AditionalInformationId { get; set; }
        public int? DatasetId { get; set; }

        public virtual DatasetDTO Dataset { get; set; }

        public virtual DataSourceAditionalInformationDTO AditionalInformation { get; set; }
    }
}