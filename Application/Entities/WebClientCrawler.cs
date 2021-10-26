using System;
using System.Net;

namespace Application.Entities
{
    public class WebClientCrawler : WebClient
    {
        internal int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri Address)
        {
            WebRequest WebReq = base.GetWebRequest(Address);
            WebReq.Timeout = Timeout * 1000; // Seconds
            return WebReq;
        }
    }
}