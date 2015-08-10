using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Domain;
using Utility.Helpers;

namespace WebProject.Controllers
{
    public class SettingsController : BaseApiController
    {
        [HttpGet]
        [Route("api/settings/setItem/{url}/{width}/{height}/{deviceType}")]
        public void SetItem(string url, int width, int height, int deviceType)
        {
            ImageTracker.SetImageSize(url.CleanUrl(), width, height, (ImageSizes) deviceType);
        }

        [HttpPost]
        [Route("api/settings/save")]
        public void SaveSettings([FromBody] IEnumerable<ImageSettings> imageSettingsList)
        {
            if (imageSettingsList != null)
            {
                foreach (ImageSettings imageSetting in imageSettingsList.Where(t => !string.IsNullOrEmpty(t.Url)))
                    ImageTracker.SetImageSize(imageSetting.Url.CleanUrl(), imageSetting.Width, imageSetting.Height, (ImageSizes) imageSetting.DeviceType);
            }
        }

        [HttpGet]
        [Route("api/settings/remove/{url}")]
        public void RemoveFromCache(string url)
        {
            ImageTracker.RemoveItemFromCache(new Uri(url));
        }
    }
}