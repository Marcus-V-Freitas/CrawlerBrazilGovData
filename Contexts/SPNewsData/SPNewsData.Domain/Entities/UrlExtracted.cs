namespace SPNewsData.Domain.Entities
{
    public class UrlExtracted
    {
        public int Id { get; private set; }
        public string Title { get; private set; }
        public string Url { get; private set; }

        public string Search { get; private set; }

        public bool ParsingLayout { get; private set; }

        public UrlExtracted()
        {
        }

        public UrlExtracted(string title, string url, string search, bool parsingLayout)
        {
            Title = title;
            Url = url;
            Search = search;
            ParsingLayout = parsingLayout;
        }
    }
}