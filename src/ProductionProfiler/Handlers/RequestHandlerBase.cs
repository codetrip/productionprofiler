using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;

namespace ProductionProfiler.Handlers
{
    public abstract class RequestHandlerBase : IRequestHandler
    {
        public void HandleRequest(HttpContext context, RequestInfo requestInfo)
        {
            string json = new JavaScriptSerializer().Serialize(GetResponseData(requestInfo));

            string path = VirtualPathUtility.ToAbsolute("~/", context.Request.ApplicationPath);

            StringBuilder response = new StringBuilder();
            response.AppendFormat("<html><head></head><body>");
            response.AppendFormat("<script type='text/javascript'>var profileData = {0}, profilePath = '{1}', profileAction = '{2}';</script>", json, path, Action(requestInfo));
            response.AppendFormat("<link href=\"profiler?r=Css.css&ct={0}\" rel=\"stylesheet\" type=\"text/css\" />", HttpUtility.UrlEncode("text/css"));
            response.AppendFormat("<script type='text/javascript' src='profiler?r=Client.js&ct={0}'></script>", HttpUtility.UrlEncode("application/javascript"));
            response.AppendFormat("<div id=\"profiler\" class=\"profiler\"></div></body></html>");

            context.Response.Write(response.ToString());
            context.Response.AddHeader("Content-Type", "text/html");
            context.Response.Cache.SetCacheability(HttpCacheability.Private);
        }

        protected abstract object GetResponseData(RequestInfo requestInfo);
        protected abstract string Action(RequestInfo requestInfo);
    }
}
