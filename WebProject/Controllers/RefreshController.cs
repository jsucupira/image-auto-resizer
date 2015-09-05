using System;
using System.Web.Http;

namespace WebProject.Controllers
{
    public class RefreshController : BaseApiController
    {
        [HttpDelete]
        [Route("api/images/clear/{url}")]
        public void Index(string url)
        {
            ImageTracker.RemoveItemFromCache(new Uri(url));
        }

        [HttpDelete]
        [Route("api/images/clear")]
        public void ClearAllImages()
        {
            ImageTracker.ClearCache();
        }
    }
}