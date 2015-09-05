using System;
using System.Web.Http;

namespace WebProject.Controllers
{
    public class RefreshController : BaseApiController
    {
        [HttpDelete]
        [Route("api/images/clear")]
        public void Index([FromUri]string url)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                ImageTracker.RemoveItemFromCache(uri);
        }

        [HttpDelete]
        [Route("api/images/clear")]
        public void ClearAllImages()
        {
            ImageTracker.ClearCache();
        }
    }
}