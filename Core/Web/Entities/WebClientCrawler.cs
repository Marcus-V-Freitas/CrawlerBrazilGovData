using System;
using System.Net;

namespace Core.Web.Entities
{
    public class WebClientCrawler : WebClient
    {
        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri Address)
        {
            WebRequest WebReq = base.GetWebRequest(Address);
            WebReq.Timeout = Timeout * 1000; // Seconds
            return WebReq;
        }
    }
}