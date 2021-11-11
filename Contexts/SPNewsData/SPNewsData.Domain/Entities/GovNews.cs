using System;
using System.Collections.Generic;

namespace SPNewsData.Domain.Entities
{
    public class GovNews
    {
        public int? Id { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public DateTime? PublicationDate { get; private set; }
        public string Source { get; private set; }
        public string Content { get; private set; }
        public string Url { get; private set; }
        public string Search { get; private set; }

        public DateTime? CaptureDate { get; private set; } = DateTime.Now;

        public virtual List<Subject> Subjects { get; private set; }
        public virtual List<Evidence> Evidences { get; private set; }

        public GovNews()
        {
            Evidences = new();
            Subjects  = new();
        }

        public GovNews(string title, string subtitle, DateTime? publicationDate,
                       string source, string content, string url, string search)
        {
            Title = title;
            Subtitle = subtitle;
            PublicationDate = publicationDate;
            Source = source;
            Content = content;
            Url = url;
            Search = search;
            Evidences = new();
            Subjects = new();
        }
    }
}