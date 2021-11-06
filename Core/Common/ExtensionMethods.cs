using Core.Web.Entities;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Common
{
    public static class ExtensionMethods
    {
        public static string CombineUrl(this string baseUrl, string relativeUrl)
        {
            UriBuilder baseUri = new UriBuilder(baseUrl);
            Uri newUri;

            if (Uri.TryCreate(baseUri.Uri, relativeUrl, out newUri))
            {
                return newUri.ToString();
            }
            else
            {
                throw new ArgumentException("Unable to combine specified url values");
            }
        }

        public static string NormalizeString(this string text)
        {
            return Regex.Replace(text, @"\r\n?|\n|[ ]{2,}", "").ToUpper().Replace("LER MAIS", string.Empty).Trim();
        }

        public static async Task<string> GetResponseHtmlAsync(this HttpClient httpClient, string url)
        {
            try
            {
                HttpResponseMessage responseMessage = await httpClient.GetAsync(url);

                if (responseMessage.IsSuccessStatusCode)
                {
                    return await responseMessage.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine($"Error occurred, the status code is: {responseMessage.StatusCode}");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        public static void AddRangeIfNotNullOrEmpty<T>(this List<T> source, IEnumerable<T> collection)
        {
            if (collection != null && collection.Any())
            {
                source.AddRange(collection);
            }
        }

        public static HtmlDocument CreateHtmlDocument(this string htmlResponse)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.OptionAutoCloseOnEnd = true;
            if (!string.IsNullOrEmpty(htmlResponse))
            {
                doc.LoadHtml(htmlResponse);
            }
            return doc;
        }

        public static string GetValueByKey(this Dictionary<string, string> dictionary, string key)
        {
            dictionary.TryGetValue(key, out string value);
            return value;
        }

        public static Dictionary<string, string> ExtractTableInfo(this string html, string nodeXPath, bool normalize = true)
        {
            Dictionary<string, string> extracted = new Dictionary<string, string>();
            var tableNode = html.CreateHtmlDocument().DocumentNode.SelectSingleNode(nodeXPath);

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

        public static string ExtractSingleInfo(this string html, string nodeXPath, bool normalize = true)
        {
            HtmlDocument doc = html.CreateHtmlDocument();
            HtmlNode node = doc.DocumentNode.SelectSingleNode(nodeXPath);

            if (node != null)
            {
                return normalize ? node.InnerText.NormalizeString() : node.InnerText;
            }

            return string.Empty;
        }

        public static string ExtractSingleInfoAttribute(this string html, string nodeXPath, string attributeName, bool normalize = true)
        {
            HtmlDocument doc = html.CreateHtmlDocument();
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

        public static bool TryConvertStringToBool(this string value)
        {
            return value == "1";
        }

        public static int TryConvertStringToInt(this string value)
        {
            int.TryParse(value, out int result);
            return result;
        }

        public static DateTime? TryConvertDatetime(this string value, string format = "dd/MM/yyyy", string culture = "")
        {
            CultureInfo cultureInfo;
            if (string.IsNullOrEmpty(culture))
            {
                cultureInfo = CultureInfo.InvariantCulture;
            }
            else
            {
                cultureInfo = CultureInfo.GetCultureInfo(culture);
            }

            if (DateTime.TryParseExact(value, format, cultureInfo, DateTimeStyles.None, out DateTime date))
            {
                return date;
            }
            return null;
        }

        public static List<string> ExtractListInfoAttributes(this string html, string nodeXPath, string attributeName, bool normalize = true)
        {
            List<string> infos = new List<string>();
            HtmlDocument doc = html.CreateHtmlDocument();
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

        public static List<string> ExtractListInfo(this string html, string nodeXPath, bool normalize = true)
        {
            List<string> infos = new List<string>();
            HtmlDocument doc = html.CreateHtmlDocument();
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

        public static string GetFileNameFromUrl(this string url)
        {
            Uri uri = new(url);
            string filename = Path.GetFileName(uri.LocalPath);

            if (filename.Contains("?"))
                filename = filename.Substring(0, filename.IndexOf("?"));

            return filename;
        }

        public static Stream DownloadFilesFromUrl(this string url)
        {
            using (WebClientCrawler webClient = new WebClientCrawler())
            {
                webClient.Timeout = 10;
                var content = webClient.DownloadData(url);
                return new MemoryStream(content);
            }
        }

        public static string DownloadFilesFromUrl(this string url, string pathSave, bool folderTypeFile = false)
        {
            string completeDownloadPath = string.Empty;

            try
            {
                using (WebClientCrawler webClient = new WebClientCrawler())
                {
                    webClient.Timeout = 10;
                    string folderType = string.Empty;
                    string filename = url.GetFileNameFromUrl();
                    string extension = filename.GetExtensionOrDefault(false, false);

                    if (folderTypeFile)
                    {
                        folderType = (string.IsNullOrEmpty(extension) ? "Others" : extension);
                    }

                    CreateDirectoryIfNotExists(Path.Combine(pathSave, folderType));

                    completeDownloadPath = Path.Combine(pathSave, folderType, $"{Guid.NewGuid()}.{extension}");

                    if (!File.Exists(completeDownloadPath))
                    {
                        webClient.DownloadFile(url, completeDownloadPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                completeDownloadPath = string.Empty;
            }
            return completeDownloadPath;
        }

        public static string GenerateRandomFileName(this string url)
        {
            string filename = url.GetFileNameFromUrl();
            string extension = filename.GetExtensionOrDefault(false, false);

            if (!string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(extension))
            {
                return $"{Guid.NewGuid()}.{extension}";
            }
            return string.Empty;
        }

        private static string GetExtensionOrDefault(this string filename, bool dot = true, bool normalize = true)
        {
            string extension = Path.GetExtension(filename);
            extension = dot ? extension : extension.Replace(".", "");
            extension = normalize ? extension.NormalizeString() : extension;

            return extension;
        }

        public static void CreateDirectoryIfNotExists(this string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static bool IsValidJson(this string json)
        {
            if (string.IsNullOrWhiteSpace(json)) { return false; }
            json = json.Trim();
            if ((json.StartsWith("{") && json.EndsWith("}")) || //For object
                (json.StartsWith("[") && json.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(json);
                    return true;
                }
                catch (JsonReaderException ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}