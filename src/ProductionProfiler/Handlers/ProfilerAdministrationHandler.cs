using System.Web;
using System.Web.SessionState;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Handlers
{
    public class ProfilerAdministrationHandler : ComponentBase, IHttpHandler, IRequiresSessionState 
    {
        public void ProcessRequest(HttpContext context)
        {
            if(ProfilerContext.Configuration.AuthorisedForManagement(context))
            {
                var requestInfo = RequestInfoParser.Parse(context.Request);
                var requestHandler = RequestHandlerFactory.Create(requestInfo);
                requestHandler.HandleRequest(context, requestInfo);
            }
            else
            {
                context.Response.Write("<html><body><h1>You are not authorized to perform the requested operation</h1></body></html>");
                context.Response.End();
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
