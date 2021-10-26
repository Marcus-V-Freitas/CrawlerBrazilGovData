using System;

namespace Domain.Entities
{
    public class DatasetAditionalInformation
    {
        public int Id { get; private set; }
        public string Source { get; private set; }
        public string Manager { get; private set; }
        public DateTime? LastUpdate { get; private set; }
        public DateTime? CreationDate { get; private set; }
        public string UpdateFrequency { get; private set; }

        public DatasetAditionalInformation()
        {
        }

        public DatasetAditionalInformation(string source, string manager, DateTime? lastUpdate, DateTime? creationDate, string updateFrequency)
        {
            Source = source;
            Manager = manager;
            LastUpdate = lastUpdate;
            CreationDate = creationDate;
            UpdateFrequency = updateFrequency;
        }
    }
}