using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Core.MEF;
using Domain;

namespace Resizing.Services
{
    [Export(typeof (IImageServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImageServices : IImageServices
    {
        private static readonly Dictionary<Uri, DownloadResponse> _savedImages = new Dictionary<Uri, DownloadResponse>();
        private static readonly Dictionary<string, DownloadResponse> _customImages = new Dictionary<string, DownloadResponse>();
        private static readonly IDevice _deviceServices = ObjectContainer.Container.GetExportedValue<IDevice>();
        private static readonly IImageRepository _imageRepository = ObjectContainer.Container.GetExportedValue<IImageRepository>();

        public async Task<Tuple<MimeTypes, string>> ProcessRequest(DownloadRequest request)
        {
            return await Task.Factory.StartNew(() => ProcessAsync(request));
        }

        public void SetImageSize(string url, int width, int height, ImageSizes deviceType)
        {
            ImageDefaults imageDefault = _imageRepository.Get(url);
            if (imageDefault != null)
            {
                if (imageDefault.ImageSizes.ContainsKey(deviceType))
                    imageDefault.ImageSizes[deviceType] = new Tuple<int, int>(width, height);
                else
                    imageDefault.ImageSizes.Add(deviceType, new Tuple<int, int>(width, height));
            }
            else
            {
                imageDefault = new ImageDefaults {Url = url};
                imageDefault.ImageSizes.Add(deviceType, new Tuple<int, int>(width, height));
            }
            _imageRepository.Save(imageDefault);

            if (_savedImages.ContainsKey(new Uri(url)))
                _savedImages.Remove(new Uri(url));
        }

        public void SetPath(string path)
        {
            DownloadProcess.SetPath(path);
        }

        public void RemoveItemFromCache(Uri url)
        {
            if (_savedImages.ContainsKey(url))
            {
                _imageRepository.Remove(url.ToString());
                _savedImages.Remove(url);

                string keyToRemove = null;
                foreach (string key in _customImages.Keys)
                {
                    if (key.StartsWith(url.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        keyToRemove = key;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(keyToRemove))
                    _customImages.Remove(keyToRemove);
            }
        }

        private static Tuple<MimeTypes, string> DecideWhichImageToUse(DownloadRequest request, DownloadResponse response)
        {
            ImageSizes imageSize = _deviceServices.GetImageSize(request.UserAgent);
            return response.ImageLocations[imageSize];
        }

        private static Tuple<MimeTypes, string> ProcessAsync(DownloadRequest request)
        {
            if (string.IsNullOrEmpty(request.Options))
            {
                if (_savedImages.ContainsKey(request.Url))
                    return DecideWhichImageToUse(request, _savedImages.First(t => t.Key == request.Url).Value);
            }
            else
            {
                if (_customImages.ContainsKey(request.Url + request.Options))
                    return _customImages[request.Url + request.Options].ImageLocations[ImageSizes.Default];
            }

            DownloadResponse response = DownloadProcess.Download(request);
            if (response != null)
            {
                if (!string.IsNullOrEmpty(request.Options))
                {
                    if (!_customImages.ContainsKey(request.Url + request.Options))
                        _customImages.Add(request.Url + request.Options, response);

                    return response.ImageLocations[ImageSizes.Default];
                }
                if (!_savedImages.ContainsKey(request.Url))
                    _savedImages.Add(request.Url, response);

                return DecideWhichImageToUse(request, response);
            }
            return null;
        }
    }
}