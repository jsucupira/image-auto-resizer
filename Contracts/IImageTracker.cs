using System;
using System.Threading.Tasks;
using Domain;

namespace Contracts
{
    public interface IImageServices
    {
        Task<Tuple<MimeTypes, string>> ProcessRequest(DownloadRequest request);
        void SetImageSize(string url, int width, int height, ImageSizes deviceType);
        void SetDirectory(string folderPath);
        void RemoveItemFromCache(Uri url);
        void ClearCache();
        void ClearSettings();
    }
}