using System;
using System.Threading.Tasks;
using Domain;

namespace Contracts
{
    public interface IImageServices
    {
        Task<Tuple<MimeTypes, string>> ProcessRequest(DownloadRequest request);
        void SetImageSize(string url, int width, int height, ImageSizes deviceType);
        void SetDirectory(IImageRepository imageRepository);
        void RemoveItemFromCache(Uri url);
    }
}