namespace Core.Configuration
{
    public class Parser
    {
        public bool GetUrlsSQS { get; set; }
        public bool GetUrlsMysql { get; set; }
        public bool SQSSave { get; set; }
        public bool DeleteAllMessagesSQS { get; set; }
        public bool DeleteQueueSQS { get; set; }
        public string BucketNameS3 { get; set; }
        public bool S3Save { get; set; }
    }
}