using System;
using System.Collections.Generic;

namespace Domain
{
    [Serializable]
    public class ImageDefaults
    {
        public ImageDefaults()
        {
            ImageSizes = new Dictionary<ImageSizes, Tuple<int, int>>(3);
        }
        public string Url { get; set; }

        public Dictionary<ImageSizes, Tuple<int, int>> ImageSizes { get; set; }
    }
}
