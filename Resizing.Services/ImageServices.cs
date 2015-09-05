using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Core.MEF;
using Domain;

namespace Resizing.Services
{
    [Export(typeof(IImageServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImageServices : IImageServices
    {
        private const string SAVED_IMAGE_SECTION = "SAVED_IMAGE_SECTION";
        private const string CUSTOM_IMAGES_SECTION = "CUSTOM_IMAGES_SECTION";
        private static readonly ICacheDataStorage _savedImages = ObjectContainer.Resolve<ICacheDataStorage>();
        private static readonly ICacheDataStorage _customImages = ObjectContainer.Resolve<ICacheDataStorage>();
        private static readonly IDevice _deviceServices = ObjectContainer.Resolve<IDevice>();
        private static readonly IImageConfigurationRepository _imageConfigurationRepository = ObjectContainer.Resolve<IImageConfigurationRepository>();

        public async Task<Tuple<MimeTypes, string>> ProcessRequest(DownloadRequest request)
        {
            return await Task.Factory.StartNew(() => ProcessAsync(request));
        }

        public void SetImageSize(Uri url, int width, int height, ImageSizes deviceType)
        {
            ImageDefaults imageDefault = _imageConfigurationRepository.Get(url.ToString());
            if (imageDefault != null)
            {
                if (imageDefault.ImageSizes.ContainsKey(deviceType))
                    imageDefault.ImageSizes[deviceType] = new Tuple<int, int>(width, height);
                else
                    imageDefault.ImageSizes.Add(deviceType, new Tuple<int, int>(width, height));
            }
            else
            {
                imageDefault = new ImageDefaults { Url = url.ToString() };
                imageDefault.ImageSizes.Add(deviceType, new Tuple<int, int>(width, height));
            }
            _imageConfigurationRepository.Save(imageDefault);
            _savedImages.Remove(url.ToString());
        }

        public void SetDirectory(string folderPath)
        {
            DownloadProcess.SetPath(folderPath);
        }

        public void RemoveItemFromCache(Uri url)
        {
            List<string> fileNames = new List<string>();
            if (_savedImages.Exists(url.ToString()))
            {
                fileNames.AddRange(DeleteFileForImageLocation(_savedImages.Get<DownloadResponse>(url.ToString()).ImageLocations));
                _savedImages.Remove(url.ToString());
            }

            if (_customImages.Exists(url.ToString()))
            {
                fileNames.AddRange(DeleteFileForImageLocation(_customImages.Get<DownloadResponse>(url.ToString()).ImageLocations));
                _customImages.Remove(url.ToString());
            }

            if (fileNames.Any())
            {
                Task.Factory.StartNew(() =>
                    Parallel.ForEach(fileNames, file =>
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                        }
                    }));
            }
        }

        public void DeleteSettings(Uri url)
        {
            _imageConfigurationRepository.Remove(url.ToString());
        }

        private static IEnumerable<string> DeleteFileForImageLocation(Dictionary<ImageSizes, Tuple<MimeTypes, string>> imageLocationDictionary)
        {
            return imageLocationDictionary.Select(imageLocation => imageLocation.Value.Item2);
        }

        public void ClearCache()
        {
            _savedImages.RemoveSection(SAVED_IMAGE_SECTION);
            _customImages.RemoveSection(CUSTOM_IMAGES_SECTION);
            DownloadProcess.ClearFolder();
        }

        public void ClearSettings()
        {
            _imageConfigurationRepository.Clear();
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
                if (_savedImages.Exists(request.Url.ToString()))
                    return DecideWhichImageToUse(request, _savedImages.Get<DownloadResponse>(request.Url.ToString()));
            }
            else
            {
                if (_customImages.Exists(request.Url + request.Options))
                    return _customImages.Get<DownloadResponse>(request.Url + request.Options).ImageLocations[ImageSizes.Default];
            }

            DownloadResponse response = DownloadProcess.Download(request);
            if (response != null)
            {
                if (!string.IsNullOrEmpty(request.Options))
                {
                    if (!_customImages.Exists(request.Url + request.Options))
                        _customImages.Add(request.Url + request.Options, response, CUSTOM_IMAGES_SECTION);

                    return response.ImageLocations[ImageSizes.Default];
                }
                if (!_savedImages.Exists(request.Url.ToString()))
                    _savedImages.Add(request.Url.ToString(), response, SAVED_IMAGE_SECTION);

                return DecideWhichImageToUse(request, response);
            }
            return null;
        }
    }
}