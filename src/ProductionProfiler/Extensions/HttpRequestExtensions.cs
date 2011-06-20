using System;
using System.Web;

namespace ProductionProfiler.Core.Extensions
{
    public static class HttpRequestExtensions
    {
        private const string XRequestedWith = "X-REQUESTED-WITH";
        private const string XForwardedFor = "HTTP_X_FORWARDED_FOR";
        private const string XRemoteAddress = "REMOTE_ADDR";
        private const string XmlHttpRequest = "XMLHttpRequest";

        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            return (request[XRequestedWith] == XmlHttpRequest) || (request.Headers[XRequestedWith] == XmlHttpRequest);
        }

        public static string ClientIpAddress(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            return string.IsNullOrEmpty(request.ServerVariables[XForwardedFor])
                ? request.ServerVariables[XRemoteAddress]
                : request.ServerVariables[XForwardedFor];
        }
    }
}
