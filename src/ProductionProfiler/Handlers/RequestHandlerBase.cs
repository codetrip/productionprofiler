using System.IO.Compression;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public abstract class RequestHandlerBase : IRequestHandler
    {
        public ProfilerConfiguration Config { protected get; set; }

        public void HandleRequest(HttpContext context, RequestInfo requestInfo)
        {
            var jsonResponse = DoHandleRequest(requestInfo);

            if(!string.IsNullOrEmpty(jsonResponse.Redirect))
            {
                context.Response.Redirect(jsonResponse.Redirect);
                context.Response.End();
                return;
            }

            string json = new JavaScriptSerializer().Serialize(jsonResponse);
            string path = VirtualPathUtility.ToAbsolute("~/", context.Request.ApplicationPath);

            StringBuilder response = new StringBuilder();
            response.AppendFormat("<html><head>");

            //response.AppendFormat("<link href=\"/Content/Resources.Css.css\" rel=\"stylesheet\" type=\"text/css\" />");
            response.AppendFormat("<link href=\"profiler?resource=Css.css&contenttype={0}\" rel=\"stylesheet\" type=\"text/css\" />", HttpUtility.UrlEncode("text/css"));

            response.AppendFormat("</head><body><span id='title'></span>");
            response.AppendFormat("<script type='text/javascript'>var profileData = {0}, profilePath = '{1}', profileAction = '{2}';</script>", json, path, requestInfo.Action);
            
            //response.AppendFormat("<script type='text/javascript' src='/Content/Resources.Client.js'></script>");
            response.AppendFormat("<script type='text/javascript' src='profiler?resource=Client.js&contenttype={0}'></script>", HttpUtility.UrlEncode("application/javascript"));
            
            response.AppendFormat("<div id=\"profiler\" class=\"container\"></div></body></html>");

            if (context.Request.Headers.Get(Constants.HttpHeaders.AcceptEncoding) != null && context.Request.Headers.Get(Constants.HttpHeaders.AcceptEncoding).Contains(Constants.RequestEncoding.GZip))
            {
                context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                context.Response.AppendHeader(Constants.HttpHeaders.ContentEncoding, Constants.RequestEncoding.GZip);
                context.Response.Cache.VaryByHeaders[Constants.HttpHeaders.AcceptEncoding] = true;
            }

            context.Response.Write(response.ToString());
            context.Response.AddHeader("Content-Type", "text/html");
            context.Response.Cache.SetCacheability(HttpCacheability.Private);
        }

        protected abstract JsonResponse DoHandleRequest(RequestInfo requestInfo);
    }
}
