namespace SPGovernmentData.Application.Entities.DTOs
{
    public class TagDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? DatasetId { get; set; }

        public virtual DatasetDTO Dataset { get; set; }
    }
}