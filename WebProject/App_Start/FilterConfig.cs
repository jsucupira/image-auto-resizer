using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SimpleLogging;

namespace WebProject
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            if (HttpContext.Current.IsDebuggingEnabled)
                filters.Add(new LogActionFilter());
        }
    }

    public class LogActionFilter : ActionFilterAttribute
    {
        private static readonly ISimpleLogger _logger = SimpleLoggerFactory.Create();

        private static void Log(RouteData routeData)
        {
            HttpRequest request = HttpContext.Current.Request;
            object controllerName = routeData.Values["controller"];
            object actionName = routeData.Values["action"];
            string referrerUrl = "N/A";
            if (HttpContext.Current.Request.UrlReferrer != null) referrerUrl = HttpContext.Current.Request.UrlReferrer.ToString();

            string message = $"controller:{controllerName} -> action:{actionName}";
            _logger.LogTrace(message, request.Url.ToString(), RequestHelpers.GetClientIpAddress(request), referrerUrl);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            Log(filterContext.RouteData);
        }
    }
}