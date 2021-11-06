using Core.Common;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace Core.Web.Entities
{
    public class HtmlString
    {
        private string _html { get; set; }

        private HtmlString(string html)
        {
            _html = html;
        }

        public static HtmlString Instance(string html = "")
        {
            return new HtmlString(html);
        }

        public string ToHtmlString { get => _html; }

        public bool IsNullOrEmpty { get => string.IsNullOrEmpty(_html); }

        public HtmlDocument CreateHtmlDocument()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.OptionAutoCloseOnEnd = true;
            if (!string.IsNullOrEmpty(ToHtmlString))
            {
                doc.LoadHtml(ToHtmlString);
            }
            return doc;
        }

        public Dictionary<string, string> ExtractTableInfo(string nodeXPath, bool normalize = true)
        {
            Dictionary<string, string> extracted = new Dictionary<string, string>();
            var tableNode = CreateHtmlDocument().DocumentNode.SelectSingleNode(nodeXPath);

            if (tableNode != null)
            {
                var thNodes = tableNode.SelectNodes(".//th");
                var tdNodes = tableNode.SelectNodes(".//td");

                List<string> terms = new List<string>() { "CAMPO", "VALOR" };
                var fieldsRemove = thNodes.Where(x => terms.Contains(x.InnerText.NormalizeString())).ToList();

                if (fieldsRemove.Any())
                {
                    foreach (var fieldRemove in fieldsRemove)
                    {
                        thNodes.Remove(fieldRemove);
                    }
                }

                if (thNodes != null && tdNodes != null && thNodes.Count() == tdNodes.Count())
                {
                    foreach (int indice in Enumerable.Range(0, thNodes.Count))
                    {
                        string key = thNodes[indice].InnerText;
                        string value = tdNodes[indice].InnerText;
                        extracted.Add(normalize ? key.NormalizeString() : key, normalize ? value.NormalizeString() : value);
                    }
                }
            }
            return extracted;
        }

        public string ExtractSingleInfo(string nodeXPath, bool normalize = true)
        {
            HtmlDocument doc = CreateHtmlDocument();
            HtmlNode node = doc.DocumentNode.SelectSingleNode(nodeXPath);

            if (node != null)
            {
                return normalize ? node.InnerText.NormalizeString() : node.InnerText;
            }

            return string.Empty;
        }

        public List<string> ExtractListInfo(string nodeXPath, bool normalize = true)
        {
            List<string> infos = new List<string>();
            HtmlDocument doc = CreateHtmlDocument();
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(nodeXPath);

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    infos.Add(normalize ? node.InnerText.NormalizeString() : node.InnerText);
                }
                return infos;
            }
            return infos;
        }

        public string ExtractSingleInfoAttribute(string nodeXPath, string attributeName, bool normalize = true)
        {
            HtmlDocument doc = CreateHtmlDocument();
            HtmlNode node = doc.DocumentNode.SelectSingleNode(nodeXPath);

            if (node != null)
            {
                string value = node.GetAttributeValue(attributeName, null);
                if (!string.IsNullOrEmpty(value))
                {
                    return normalize ? value.NormalizeString() : value;
                }
            }

            return string.Empty;
        }

        public List<string> ExtractListInfoAttributes(string nodeXPath, string attributeName, bool normalize = true)
        {
            List<string> infos = new List<string>();
            HtmlDocument doc = CreateHtmlDocument();
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(nodeXPath);

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    string value = node.GetAttributeValue(attributeName, null);
                    if (!string.IsNullOrEmpty(value))
                    {
                        infos.Add(normalize ? value.NormalizeString() : value);
                    }
                }
                return infos;
            }
            return infos;
        }

        public HtmlNodeCollection ExtractListNodes(string nodeXPath)
        {
            return CreateHtmlDocument().DocumentNode.SelectNodes(nodeXPath);
        }

        public HtmlNode ExtractSingleNode(string nodeXPath)
        {
            return CreateHtmlDocument().DocumentNode.SelectSingleNode(nodeXPath);
        }
    }
}