using System;
using System.Collections.Generic;

namespace Domain
{
    public class DownloadResponse
    {
        public DownloadResponse(string initialLocation)
        {
            ImageLocations = new Dictionary<ImageSizes, Tuple<MimeTypes, string>>
            {
                { ImageSizes.Default, new Tuple<MimeTypes, string>(new MimeTypes(new Uri(initialLocation)), initialLocation) }
            };
        }

        public Dictionary<ImageSizes, Tuple<MimeTypes, string>> ImageLocations { get; set; }
        public bool IsCached { get; set; }
    }
}