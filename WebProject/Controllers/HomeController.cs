using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Domain;
using Utility;

namespace WebProject.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home
        [HttpGet]
        [Route("~/{url}/{options}")]
        [Route("~/")]
        public async Task GetImage(string url, string options)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                DownloadRequest request = new DownloadRequest
                {
                    Url = uri,
                    ReferrerUrl = Request.UrlReferrer,
                    UserAgent = Request.UserAgent,
                    Options = options.FilterOptions()
                };
                Task<Tuple<MimeTypes, string>> processTask = ImageTracker.ProcessRequest(request);
                Tuple<MimeTypes, string> result = await processTask;
                Response.PublicCache(60);
                if (result != null)
                {
                    Response.WriteFile(result.Item2);
                    Response.ContentType = result.Item1.ToString();
                }
                else
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(uri);
                    using (HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse())
                    using (Stream stream = httpWebResponse.GetResponseStream())
                    {
                        if (stream != Stream.Null)
                        {
                            Response.BinaryWrite(StreamHelper.ReadToEnd(stream));
                            Response.ContentType = Utility.Helper.GetImageType(request).Item1.ToString();
                        }
                    }
                }
            }
            else
                Response.Redirect("~/Error");
        }

        [Route("~/error")]
        public ActionResult Error()
        {
            return View();
        }
    }
}