namespace SPGovernmentData.Domain.Entities
{
    public class Tag
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int? DatasetId { get; private set; }

        public virtual Dataset Dataset { get; private set; }

        public Tag()
        {
        }

        public Tag(string name, int? datasetId)
        {
            Name = name;
            DatasetId = datasetId;
        }
    }
}