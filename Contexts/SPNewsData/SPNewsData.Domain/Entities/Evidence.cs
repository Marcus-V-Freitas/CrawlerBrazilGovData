using SPNewsData.Domain.Enums;

namespace SPNewsData.Domain.Entities
{
    public class Evidence
    {
        public int? Id { get; private set; }
        public string RawContent { get; private set; }
        public EvidenceType? EvidenceType { get; private set; }
        public int? GovNewsId { get; private set; }

        public virtual GovNews GovNews { get; private set; }

        public Evidence()
        {
        }

        public Evidence(string rawContent, EvidenceType? evidenceType, int? govNewsId)
        {
            EvidenceType = evidenceType;
            RawContent = rawContent;
            GovNewsId = govNewsId;
        }
    }
}