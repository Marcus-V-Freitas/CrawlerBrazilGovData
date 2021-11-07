namespace SPNewsData.Application.Entities.DTOs
{
    public class SubjectDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int? GovNewsId { get; set; }

        public virtual GovNewsDTO GovNews { get; set; }
    }
}