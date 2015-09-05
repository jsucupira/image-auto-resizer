using System;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Contracts;
using Core.MEF;
using Domain;
using ImageResizer;

namespace Resizing.Services
{
    public static class DownloadProcess
    {
        private static readonly IImageConfigurationRepository _imageConfigurationRepository = ObjectContainer.Container.GetExportedValue<IImageConfigurationRepository>();
        private static IImageRepository _imageRepository;
        private static IImageRepository _fileSystemRepository = ObjectContainer.Container.ResolveCustomExportValue<IImageRepository>("FileSystem");
        private const string TEMP_LOCATION = @"c:\temp\{0}";

        public static DownloadResponse Download(DownloadRequest request)
        {
            try
            {
                int sourceWidth = 0;
                int sourceHeight = 0;

                string imageName = request.Url.Segments[request.Url.Segments.Length - 1];
                imageName = imageName.ToLower();
                string initial = imageName;
                initial = initial.ToLower();

                DownloadResponse response = new DownloadResponse(initial);
                Tuple<ImageFormat, string> format = Utility.Helpers.Utility.GetImageType(request);
                ImageFormat imageFormat = format.Item1;
                string extension = format.Item2;

                if (!_imageRepository.Exists(initial))
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(request.Url);
                    using (HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse())
                    using (Stream stream = httpWebResponse.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            using (System.Drawing.Image image = System.Drawing.Image.FromStream(stream))
                            {
                                sourceWidth = image.Width; //store original width of source image.
                                sourceHeight = image.Height; //store original height of source image.
                                image.Save(string.Format(TEMP_LOCATION, initial), imageFormat);
                                _imageRepository.SaveFile(initial);
                            }
                        }
                    }
                }
                else
                {
                    using (System.Drawing.Image image = System.Drawing.Image.FromFile(initial))
                    {
                        sourceWidth = image.Width; //store original width of source image.
                        sourceHeight = image.Height; //store original height of source image.
                    }
                }

                //Task.Factory.StartNew(() =>
                //{
                try
                {
                    if (!string.IsNullOrEmpty(request.Options))
                    {
                        Instructions instructions = new Instructions(request.Options);

                        string fileName = string.Concat(Guid.NewGuid(), extension);
                        ImageJob job = ImageBuilder.Current.Build(new ImageJob(string.Format(TEMP_LOCATION, initial), string.Format(TEMP_LOCATION, fileName), instructions));
                        response.ImageLocations[ImageSizes.Default] = new Tuple<MimeTypes, string>(new MimeTypes(fileName), job.FinalPath);
                        _imageRepository.SaveFile(initial);
                    }
                    else
                    {
                        if (sourceWidth != 0 && sourceHeight != 0)
                        {
                            ImageDefaults preDefinedValues = _imageConfigurationRepository.Get(request.Url.ToString());
                            int initialPercentage = 60;
                            for (int i = 0; i < 2; i++)
                            {
                                float nPercent = ((float) initialPercentage/100);

                                string fileName = string.Concat(Guid.NewGuid(), extension);

                                Instructions instructions = new Instructions();
                                Tuple<int, int> predefinedItems;
                                if (preDefinedValues != null && preDefinedValues.ImageSizes.TryGetValue((ImageSizes) i, out predefinedItems))
                                {
                                    if (predefinedItems.Item1 > 0)
                                        instructions.Width = predefinedItems.Item1;
                                    if (predefinedItems.Item2 > 0)
                                        instructions.Height = predefinedItems.Item2;

                                    if (predefinedItems.Item1 > 0 && predefinedItems.Item2 > 0)
                                        instructions.Mode = FitMode.Max;
                                }
                                else
                                    instructions.Width = (int) (sourceWidth*nPercent);

                                //Let the image builder add the correct extension based on the output file type 
                                if (extension == ".jpg" || extension == ".jpeg")
                                    instructions.JpegQuality = 90;
                                ImageJob job = ImageBuilder.Current.Build(new ImageJob(string.Format(TEMP_LOCATION, initial), string.Format(TEMP_LOCATION, fileName), instructions));
                                _imageRepository.SaveFile(fileName);
                                _fileSystemRepository.DeleteFile(string.Format(TEMP_LOCATION, fileName));

                                if (response.ImageLocations.ContainsKey((ImageSizes) i))
                                    response.ImageLocations[(ImageSizes) i] = new Tuple<MimeTypes, string>(new MimeTypes(fileName), job.FinalPath);
                                else
                                    response.ImageLocations.Add((ImageSizes) i, new Tuple<MimeTypes, string>(new MimeTypes(fileName), job.FinalPath));
                                initialPercentage += 20;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Log errors
                }

                _fileSystemRepository.DeleteFile(string.Format(TEMP_LOCATION, initial));
                //});
                return response;
            }
            catch (Exception exception)
            {
                //TODO: Log Error
            }
            return null;
        }

        internal static void SetDirectory(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }
    }
}