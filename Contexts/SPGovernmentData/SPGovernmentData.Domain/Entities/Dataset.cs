using System.Collections.Generic;

namespace SPGovernmentData.Domain.Entities
{
    public class Dataset
    {
        public int Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }

        public int? Followers { get; private set; }

        public string Organization { get; private set; }

        public string License { get; private set; }

        public int? AditionalInformationId { get; private set; }

        public virtual List<Tag> Tags { get; private set; }

        public virtual List<DataSource> DataSources { get; private set; }

        public virtual DatasetAditionalInformation AditionalInformation { get; private set; }

        public Dataset()
        {
        }

        public Dataset(string title, string description, int? followers, string organization, string license, int? aditionalInformationId)
        {
            Title = title;
            Description = description;
            Followers = followers;
            Organization = organization;
            License = license;
            AditionalInformationId = aditionalInformationId;
        }
    }
}