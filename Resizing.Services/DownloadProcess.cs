using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Contracts;
using Core.MEF;
using Domain;
using ImageResizer;
using Utility;

namespace Resizing.Services
{
    public static class DownloadProcess
    {
        private static readonly IImageConfigurationRepository _imageConfigurationRepository = ObjectContainer.Resolve<IImageConfigurationRepository>();
        private static string _imageLocation;

        public static DownloadResponse Download(DownloadRequest request)
        {
            try
            {
                int sourceWidth = 0;
                int sourceHeight = 0;

                string fileLocation = Path.GetFullPath(_imageLocation);
                string imageName = request.Url.Segments[request.Url.Segments.Length - 1];
                imageName = imageName.ToLower();
                string initial = string.Concat(fileLocation, imageName);
                initial = initial.ToLower();

                DownloadResponse response = new DownloadResponse(initial);
                Tuple<ImageFormat, string> format = Helper.GetImageType(request);
                ImageFormat imageFormat = format.Item1;
                string extension = format.Item2;

                if (!File.Exists(initial))
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(request.Url);
                    using (HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse())
                    using (Stream stream = httpWebResponse.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            using (Image image = Image.FromStream(stream))
                            {
                                sourceWidth = image.Width; //store original width of source image.
                                sourceHeight = image.Height; //store original height of source image.
                                image.Save(initial, imageFormat);
                            }
                        }
                    }
                }
                else
                {
                    using (Image image = Image.FromFile(initial))
                    {
                        sourceWidth = image.Width; //store original width of source image.
                        sourceHeight = image.Height; //store original height of source image.
                    }
                }

                try
                {
                    if (!string.IsNullOrEmpty(request.Options))
                    {
                        Instructions instructions = new Instructions(request.Options);

                        string fileName = string.Concat(fileLocation, Guid.NewGuid(), extension);
                        ImageJob job = ImageBuilder.Current.Build(new ImageJob(initial, fileName, instructions));
                        response.ImageLocations[ImageSizes.Default] = new Tuple<MimeTypes, string>(new MimeTypes(fileName), job.FinalPath);
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

                                string fileName = string.Concat(fileLocation, Guid.NewGuid(), extension);

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
                                ImageJob job = ImageBuilder.Current.Build(new ImageJob(initial, fileName, instructions));

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
                return response;
            }
            catch (Exception exception)
            {
                //TODO: Log Error
            }
            return null;
        }

        internal static void SetPath(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists)
                directory.Create();
            _imageLocation = path;
        }

        internal static void ClearFolder()
        {
            foreach (string file in Directory.GetFiles(_imageLocation))
                File.Delete(file);
        }
    }
}