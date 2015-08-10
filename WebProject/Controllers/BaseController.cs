using System.Web.Http;
using System.Web.Mvc;
using Contracts;
using Core.MEF;

namespace WebProject.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IImageServices ImageTracker = ObjectContainer.Container.GetExportedValue<IImageServices>();
    }
    public class BaseApiController : ApiController
    {
        protected readonly IImageServices ImageTracker = ObjectContainer.Container.GetExportedValue<IImageServices>();
    }
}