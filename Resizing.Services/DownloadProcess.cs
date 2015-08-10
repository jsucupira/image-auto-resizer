using System;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Contracts;
using Core.MEF;
using Domain;
using ImageResizer;
using Utility.Helpers;

namespace Resizing.Services
{
    public static class DownloadProcess
    {
        private static readonly IImageRepository _imageRepository = ObjectContainer.Container.GetExportedValue<IImageRepository>();
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
                    using (HttpWebResponse httpWebReponse = (HttpWebResponse) httpWebRequest.GetResponse())
                    using (Stream stream = httpWebReponse.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            using (System.Drawing.Image image = System.Drawing.Image.FromStream(stream))
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

                        string fileName = string.Concat(fileLocation, Guid.NewGuid(), extension);
                        ImageJob job = ImageBuilder.Current.Build(new ImageJob(initial, fileName, instructions));
                        response.ImageLocations[ImageSizes.Default] = new Tuple<MimeTypes, string>(new MimeTypes(fileName), job.FinalPath);
                    }
                    else
                    {
                        if (sourceWidth != 0 && sourceHeight != 0)
                        {
                            ImageDefaults preDefinedValues = _imageRepository.Get(request.Url.ToString());
                            int initialPercentage = 60;
                            for (int i = 0; i < 2; i++)
                            {
                                float nPercent = ((float) initialPercentage/100);

                                string fileName = string.Concat(fileLocation, Guid.NewGuid(), extension);

                                Instructions instructions = new Instructions();
                                Tuple<int, int> predifinedItems;
                                if (preDefinedValues != null && preDefinedValues.ImageSizes.TryGetValue((ImageSizes) i, out predifinedItems))
                                {
                                    if (predifinedItems.Item1 > 0)
                                        instructions.Width = predifinedItems.Item1;
                                    if (predifinedItems.Item2 > 0)
                                        instructions.Height = predifinedItems.Item2;

                                    if (predifinedItems.Item1 > 0 && predifinedItems.Item2 > 0)
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
                //});
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
    }
}