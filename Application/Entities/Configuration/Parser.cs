namespace Application.Entities.Configuration
{
    public class Parser
    {
        public bool GetUrlsSQS { get; set; }
        public bool GetUrlsMysql { get; set; }
        public bool SQSSave { get; set; }
        public bool DeleteAllMessagesSQS { get; set; }
        public bool DeleteQueueSQS { get; set; }
    }
}