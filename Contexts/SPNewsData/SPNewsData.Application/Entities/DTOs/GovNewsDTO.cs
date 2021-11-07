using System;
using System.Collections.Generic;

namespace SPNewsData.Application.Entities.DTOs
{
    public class GovNewsDTO
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public DateTime? PublicationDate { get; set; }
        public string Source { get; set; }
        public string Content { get; set; }
        public DateTime? CaptureDate { get; set; } = DateTime.Now;

        public virtual List<SubjectDTO> Subjects { get; set; }
    }
}