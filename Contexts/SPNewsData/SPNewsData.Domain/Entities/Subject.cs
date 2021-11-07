namespace SPNewsData.Domain.Entities
{
    public class Subject
    {
        public int? Id { get; private set; }
        public string Name { get; private set; }
        public int? GovNewsId { get; private set; }

        public virtual GovNews GovNews { get; private set; }

        public Subject()
        {
        }

        public Subject(string name, int? govNewsId)
        {
            Name = name;
            GovNewsId = govNewsId;
        }
    }
}