namespace SPNewsData.Application.Entities.DTOs
{
    public class EvidenceDTO
    {
        public int? Id { get; set; }
        public string RawContent { get; set; }
        public int? EvidenceType { get; set; }
        public int? GovNewsId { get; set; }

        public virtual GovNewsDTO GovNews { get; set; }
    }
}