using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Domain;
using Utility;

namespace WebProject.Controllers
{
    public class SettingsController : BaseApiController
    {
        [HttpPut]
        [Route("api/settings/setItem/{url}/{width}/{height}/{deviceType}")]
        public void SetItem([FromUri]string url, [FromUri]int width, [FromUri]int height, [FromUri]int deviceType)
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

        [HttpDelete]
        [Route("api/settings/{url}")]
        public void RemoveFromCache([FromUri]string url)
        {
            ImageTracker.RemoveItemFromCache(new Uri(url));
        }

        [HttpDelete]
        [Route("api/settings/clear")]
        public void ClearSettings()
        {
            ImageTracker.ClearSettings();
        }
    }
}