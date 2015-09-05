using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Domain;

namespace Utility.Helpers
{
    public static class Utility
    {
        private static readonly List<string> _optionsList = new List<string>
        {
            "width",
            "height",
            "quality"
        };

        public static string GetPhysicalPathForFolder(this string path, bool isVirtual = true)
        {
            HttpContext current = HttpContext.Current;
            if (current == null)
                return null;

            if (!isVirtual)
                path = path.Replace(GetBaseUrl(current.Request, false), "");

            string physical = string.Format("{0}", HttpContext.Current.Server.MapPath("~/" + path));

            if (!Directory.Exists(physical))
                throw new InvalidOperationException("Physical path is not within the application root");

            return physical;
        }

        private static string GetBaseUrl(this HttpRequest request, bool appendTrailingSlash)
        {
            if (request.ApplicationPath != null)
            {
                Uri url = request.Url;
                string applicationPath = VirtualPathUtility.RemoveTrailingSlash(request.ApplicationPath);
                string baseUrl = string.Format("{0}://{1}{2}", url.Scheme, url.Authority, applicationPath);

                if (appendTrailingSlash)
                    return VirtualPathUtility.AppendTrailingSlash(baseUrl);

                return VirtualPathUtility.RemoveTrailingSlash(baseUrl);
            }
            return string.Empty;
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static string CleanUrl(this string url)
        {
            Uri uri = new Uri(url);
            return string.Format("{0}://{1}{2}", uri.Scheme, uri.Host,
                uri.AbsolutePath);
        }

        public static string FilterOptions(this string options)
        {
            if (!string.IsNullOrEmpty(options))
            {
                List<string> optionArrays = options.Split(',').ToList();
                for (int i = 0; i < optionArrays.Count; i++)
                {
                    string[] optionArray = optionArrays[i].Split('=');
                    if (!_optionsList.Contains(optionArray[0], StringComparer.OrdinalIgnoreCase))
                        optionArrays.RemoveAt(i);
                }
                return string.Join("&", optionArrays);
            }
            return options;
        }


        public static Tuple<ImageFormat, string> GetImageType(DownloadRequest request)
        {
            string imageName = request.Url.Segments[request.Url.Segments.Length - 1];
            imageName = imageName.ToLower();
            return DecideImageFormat(imageName);
        }

        private static Tuple<ImageFormat, string> DecideImageFormat(string imageName)
        {
            if (imageName.ToLower().Contains(".jpg") || imageName.ToLower().Contains(".jpeg"))
                return new Tuple<ImageFormat, string>(ImageFormat.Jpeg, ".jpg");
            if (imageName.ToLower().Contains(".png"))
                return new Tuple<ImageFormat, string>(ImageFormat.Png, ".png");
            if (imageName.ToLower().Contains(".gif"))
                return new Tuple<ImageFormat, string>(ImageFormat.Gif, ".gif");

            return new Tuple<ImageFormat, string>(ImageFormat.Jpeg, ".jpg");
        }
    }
}