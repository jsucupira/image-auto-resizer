using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Core.MEF;
using Domain;
using SimpleLogging;

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
        private static readonly ISimpleLogger _logger = SimpleLoggerFactory.Create();

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
            _savedImages.Remove(url.ToString(), SAVED_IMAGE_SECTION);
        }

        public void SetDirectory(string folderPath)
        {
            DownloadProcess.SetPath(folderPath);
        }

        public void RemoveItemFromCache(Uri url)
        {
            List<string> fileNames = new List<string>();
            if (_savedImages.Exists(url.ToString(), SAVED_IMAGE_SECTION))
            {
                fileNames.AddRange(DeleteFileForImageLocation(_savedImages.Get<DownloadResponse>(url.ToString(), SAVED_IMAGE_SECTION).ImageLocations));
                _savedImages.Remove(url.ToString(), SAVED_IMAGE_SECTION);
            }

            if (_customImages.Exists(url.ToString(), CUSTOM_IMAGES_SECTION))
            {
                fileNames.AddRange(DeleteFileForImageLocation(_customImages.Get<DownloadResponse>(url.ToString(), CUSTOM_IMAGES_SECTION).ImageLocations));
                _customImages.Remove(url.ToString(), CUSTOM_IMAGES_SECTION);
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
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Resizing.Services.ImageServices.RemoveItemFromCache");
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
            _savedImages.RemoveAll(SAVED_IMAGE_SECTION);
            _customImages.RemoveAll(CUSTOM_IMAGES_SECTION);
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
                if (_savedImages.Exists(request.Url.ToString(), SAVED_IMAGE_SECTION))
                    return DecideWhichImageToUse(request, _savedImages.Get<DownloadResponse>(request.Url.ToString(), SAVED_IMAGE_SECTION));
            }
            else
            {
                if (_customImages.Exists(request.Url + request.Options, CUSTOM_IMAGES_SECTION))
                    return _customImages.Get<DownloadResponse>(request.Url + request.Options, CUSTOM_IMAGES_SECTION).ImageLocations[ImageSizes.Default];
            }

            DownloadResponse response = DownloadProcess.Download(request);
            if (response != null)
            {
                if (!string.IsNullOrEmpty(request.Options))
                {
                    if (!_customImages.Exists(request.Url + request.Options, CUSTOM_IMAGES_SECTION))
                        _customImages.Add(request.Url + request.Options, response, CUSTOM_IMAGES_SECTION);

                    return response.ImageLocations[ImageSizes.Default];
                }
                if (!_savedImages.Exists(request.Url.ToString(), SAVED_IMAGE_SECTION))
                    _savedImages.Add(request.Url.ToString(), response, SAVED_IMAGE_SECTION);

                return DecideWhichImageToUse(request, response);
            }
            return null;
        }
    }
}