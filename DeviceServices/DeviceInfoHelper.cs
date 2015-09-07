using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Domain;
using WURFL;
using WURFL.Config;
using IDevice = Contracts.IDevice;

namespace DeviceServices
{
    [Export(typeof (IDevice))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DeviceInfoHelper : IDevice
    {
        public DeviceInfoHelper()
        {
            try
            {
                string wurflDataFile;
                if (HttpContext.Current != null)
                    wurflDataFile = HttpContext.Current.Server.MapPath("~/App_Data/");
                else
                    wurflDataFile = "../../app_data/";
                DirectoryInfo directory = new DirectoryInfo(wurflDataFile);
                if (!directory.Exists)
                    directory.Create();

                wurflDataFile += "wurfl-latest.zip";

                Assembly asm = Assembly.GetExecutingAssembly();

                //will loop tru all assembly files
                string wurflLatest = asm.GetManifestResourceNames().FirstOrDefault(n => n.Contains("wurfl-latest.zip"));
                if (wurflLatest != null)
                {
                    Stream stream = asm.GetManifestResourceStream(wurflLatest);
                    using (FileStream fileStream = new FileStream(wurflDataFile, FileMode.Create))
                        stream?.CopyTo(fileStream);
                }

                //string wurflPatchFile = HttpContext.Current.Server.MapPath("~/App_Data/web_browsers_patch.xml");
                IWURFLConfigurer configurer = new InMemoryConfigurer().MainFile(wurflDataFile); //.PatchFile(wurflPatchFile);
                WURFLManagerBuilder.Build(configurer);
            }
            catch (Exception)
            {
                // There was an issue creating the table.  Probably because the tables has aspnet_ attached to the tables names
                // Just rolling back
            }
        }

        public ImageSizes GetImageSize(string userAgent)
        {
            WURFL.IDevice deviceType = WURFLManagerBuilder.Instance.GetDeviceForRequest(userAgent, MatchMode.Accuracy);
            if (deviceType.GetVirtualCapability("is_full_desktop") == "true")
                return ImageSizes.Default;

            string mobile = deviceType.GetVirtualCapability("is_mobile");
            string tablet = deviceType.GetCapability("is_tablet");
            if (mobile == "true" && tablet == "false")
                return ImageSizes.Small;

            if (deviceType.GetCapability("is_tablet") == "true")
                return ImageSizes.Medium;

            return ImageSizes.Default;
        }
    }
}