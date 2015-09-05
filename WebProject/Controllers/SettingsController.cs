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
        [Route("api/settings")]
        public void SetItem([FromUri]string url, [FromUri]int width, [FromUri]int height, [FromUri]int deviceType)
        {
            Uri uri;
            if (Uri.TryCreate(url.CleanUrl(), UriKind.Absolute, out uri))
                ImageTracker.SetImageSize(uri, width, height, (ImageSizes) deviceType);
        }

        [HttpPut]
        [Route("api/settings/save")]
        public void SaveSettings([FromBody] IEnumerable<ImageSettings> imageSettingsList)
        {
            if (imageSettingsList != null)
            {
                foreach (ImageSettings imageSetting in imageSettingsList.Where(t => !string.IsNullOrEmpty(t.Url)))
                {
                    Uri uri;
                    if (Uri.TryCreate(imageSetting.Url.CleanUrl(), UriKind.Absolute, out uri))
                        ImageTracker.SetImageSize(uri, imageSetting.Width, imageSetting.Height, (ImageSizes) imageSetting.DeviceType);
                }
            }
        }

        [HttpDelete]
        [Route("api/settings")]
        public void RemoveFromCache([FromUri]string url)
        {
            Uri uri;
            if (Uri.TryCreate(url.CleanUrl(), UriKind.Absolute, out uri))
                ImageTracker.DeleteSettings(uri);
        }

        [HttpDelete]
        [Route("api/settings/clear")]
        public void ClearSettings()
        {
            ImageTracker.ClearSettings();
        }
    }
}