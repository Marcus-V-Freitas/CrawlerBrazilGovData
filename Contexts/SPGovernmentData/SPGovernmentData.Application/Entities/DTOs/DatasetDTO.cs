using System.Collections.Generic;

namespace SPGovernmentData.Application.Entities.DTOs
{
    public class DatasetDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int? Followers { get; set; }

        public string Organization { get; set; }

        public string License { get; set; }

        public int? AditionalInformationId { get; set; }

        public virtual List<TagDTO> Tags { get; set; }

        public virtual List<DataSourceDTO> DataSources { get; set; }

        public virtual DatasetAditionalInformationDTO AditionalInformation { get; set; }
    }
}