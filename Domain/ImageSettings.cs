using System;

namespace Domain
{
    [Serializable]
    public class ImageSettings
    {
        public int DeviceType { get; set; }
        public int Height { get; set; }
        public string Url { get; set; }
        public int Width { get; set; }
    }
}