using System;

namespace Application.Entities.DTOs
{
    public class DatasetAditionalInformationDTO
    {
        public int Id { get; set; }
        public string Source { get; set; }
        public string Manager { get; set; }
        public DateTime? LastUpdate { get; set; }
        public DateTime? CreationDate { get; set; }
        public string UpdateFrequency { get; set; }
    }
}