using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using SimpleLogging;

namespace WebProject
{
    public class ControllerActionInvoker : ApiControllerActionInvoker
    {
        private static readonly ISimpleLogger _logger = SimpleLoggerFactory.Create();

        public override Task<HttpResponseMessage> InvokeActionAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            string name = actionContext.Request.RequestUri.AbsolutePath;
            string method = actionContext.Request.Method.ToString();
            string requestUrl = actionContext.Request.RequestUri.ToString();
            string referrerUrl = "N/A";
            if (HttpContext.Current.Request.UrlReferrer != null) referrerUrl = HttpContext.Current.Request.UrlReferrer.ToString();

            Task<HttpResponseMessage> result = base.InvokeActionAsync(actionContext, cancellationToken);

            if (result.Exception != null)
            {
                var baseException = result.Exception.GetBaseException();
                _logger.LogError(baseException, "InvokeActionAsync", requestUrl, GetClientIpAddress(actionContext.Request), referrerUrl);
            }

            if (HttpContext.Current.IsDebuggingEnabled)
            {
                _logger.LogTrace($"InvokeActionAsync -> {method} -> {name}.", requestUrl, GetClientIpAddress(actionContext.Request), referrerUrl);
            }

            return result;
        }

        private static string GetClientIpAddress(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;

            if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
                return ((RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name]).Address;

            return "IP Address Unavailable";    //here the user can return whatever they like
        }
    }
}