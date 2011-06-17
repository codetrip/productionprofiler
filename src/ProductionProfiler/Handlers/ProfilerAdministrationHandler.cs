using System.Web;

namespace ProductionProfiler.Handlers
{
    public class ProfilerAdministrationHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var requestInfo = RequestInfoParser.Parse(context.Request);
            var requestHandler = RequestHandlerFactory.Create(requestInfo);
            requestHandler.HandleRequest(context, requestInfo);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
