using System;
using System.Web.Http;

namespace WebProject.Controllers
{
    public class RefreshController : BaseApiController
    {
        [HttpDelete]
        [Route("api/delete/{url}")]
        public void Index(string url)
        {
            ImageTracker.RemoveItemFromCache(new Uri(url));
        }
    }
}