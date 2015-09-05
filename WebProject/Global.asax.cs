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
using Contracts;
using Core.Azure;
using Core.MEF;

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
            {
                path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["libraryLocations"]));
            }
            else
                throw new ApplicationException("The library location needs to be specified.");

            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(path, "*.dll"));
            ObjectContainer.SetContainer(new CompositionContainer(catalog));

            IImageServices imageServices = ObjectContainer.Container.GetExportedValue<IImageServices>();
            IImageRepository cacheRepository = ObjectContainer.Container.ResolveCustomExportValue<IImageRepository>("BlobStorage");
            cacheRepository.SetFolderName("cache");
            imageServices.SetDirectory(cacheRepository);
        }
    }
}