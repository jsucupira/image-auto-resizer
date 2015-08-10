using System;
using System.Web;

namespace WebProject.Controllers
{
    public static class Helper
    {
        public static void PublicCache(this HttpResponseBase response, int maxAge)
        {
            response.Cache.SetCacheability(HttpCacheability.Private);
            response.Cache.SetMaxAge(TimeSpan.FromSeconds(maxAge));
        }
    }
}