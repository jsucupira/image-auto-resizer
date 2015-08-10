using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AzureBlobHelper;
using Contracts;
using Core.MEF;
using Utility.Helpers;

namespace WebProject
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            try
            {
                if (Directory.Exists("/cachedImages/".GetPhysicalPathForFolder(true)))
                {
                    foreach (string file in Directory.GetFiles("/cachedImages/".GetPhysicalPathForFolder(true)))
                    {
                        if (!file.Contains("placeholder.txt"))
                            File.Delete(file);
                    }
                }
                else
                    Directory.CreateDirectory("".GetPhysicalPathForFolder(true) + "cachedImages");
            }
            catch (Exception)
            {
                //Path doesn't exists
                Directory.CreateDirectory("".GetPhysicalPathForFolder(true) + "cachedImages");
            }

            try
            {
                if (Directory.Exists("/savedsettings/".GetPhysicalPathForFolder(true)))
                {
                    foreach (string file in Directory.GetFiles("/savedsettings/".GetPhysicalPathForFolder(true)))
                    {
                        if (!file.Contains("placeholder.txt"))
                            File.Delete(file);
                    }
                }
                else
                    Directory.CreateDirectory("".GetPhysicalPathForFolder(true) + "savedsettings");
            }
            catch (Exception)
            {
                //Path doesn't exists
                Directory.CreateDirectory("".GetPhysicalPathForFolder(true) + "savedsettings");
            }

            string path = ConfigurationManager.AppSettings["blobStorage"];
            if (!string.IsNullOrEmpty(path))
            {
                AzureBlobUtil azureBlobUtil = new AzureBlobUtil(path);
                path = Server.MapPath("~/loadedDlls");
                List<string> files = azureBlobUtil.BlobList("images-resizer").ToList();
                if (files.Any())
                {
                    DirectoryInfo directory = new DirectoryInfo(path);
                    if (directory.Exists)
                    {
                        foreach (FileInfo dirFiles in directory.GetFiles("*.dll"))
                            dirFiles.Delete();
                    }
                    else
                        directory.Create();

                    foreach (string file in files)
                        azureBlobUtil.DownloadBlobAsFile("images-resizer", path, file);
                }
            }
            else if (string.IsNullOrEmpty(path))
                path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["libraryLocations"]));
            else
                throw new ApplicationException("The library location needs to be specified.");

            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(path, "Caching.dll"));
            catalog.Catalogs.Add(new DirectoryCatalog(path, "DeviceServices.dll"));
            catalog.Catalogs.Add(new DirectoryCatalog(path, "Repository.dll"));
            catalog.Catalogs.Add(new DirectoryCatalog(path, "Resizing.Services.dll"));
            ObjectContainer.SetContainer(new CompositionContainer(catalog));

            IImageServices imageServices = ObjectContainer.Container.GetExportedValue<IImageServices>();
            imageServices.SetPath("/cachedImages/".GetPhysicalPathForFolder(true));
        }
    }
}