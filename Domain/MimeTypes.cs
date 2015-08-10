using System;

namespace Domain
{
    public class MimeTypes
    {
        private readonly string _imageName;

        public MimeTypes(Uri url)
        {
            string imageName = url.Segments[url.Segments.Length - 1];
            _imageName = imageName.ToLower();
        }

        public MimeTypes(string imageName)
        {
            _imageName = imageName;
        }

        private static string DecimeMimeType(string imageName)
        {
            if (imageName.Contains(".jpg") || imageName.Contains(".jpeg"))
                return "image/jpeg";
            if (imageName.Contains(".png"))
                return "image/png";
            if (imageName.Contains(".gif"))
                return "image/gif";
            if (imageName.Contains(".bmp"))
                return "image/bmp";
            if (imageName.Contains(".emf"))
                return "application/emf";
            if (imageName.Contains(".wmf"))
                return "application/wmf";
            if (imageName.Contains(".tiff"))
                return "image/tiff";
            if (imageName.Contains(".exif"))
                return "application/exif";

            else
                return "Image/x-ico";
        }

        public override string ToString()
        {
            return DecimeMimeType(_imageName);
        }
    }
}
