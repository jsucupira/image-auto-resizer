using System;

namespace Domain
{
    public class DownloadRequest
    {
        public Uri Url { get; set; }
        public string UserAgent { get; set; }
        public Uri ReferrerUrl { get; set; }
        public string Options { get; set; }
    }
}