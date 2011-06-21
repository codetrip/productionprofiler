using System.Text;
using System.Web;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Handlers
{
    public class ViewResponseRequestHandler : IRequestHandler
    {
        private readonly IProfilerRepository _repository;

        public ViewResponseRequestHandler(IProfilerRepository repository)
        {
            _repository = repository;
        }

        public void HandleRequest(HttpContext context, RequestInfo requestInfo)
        {
            var response = _repository.GetResponseById(requestInfo.Id);

            StringBuilder output = new StringBuilder();

            if (response == null)
            {
                output.AppendFormat("<html><head></head><body>No response found for Request Id '{0}'</body></html>".FormatWith(requestInfo.Id));
            }
            else
            {
                output.Append(response.Body);
            }

            context.Response.Write(output.ToString());
            context.Response.AddHeader("Content-Type", "text/html");
            context.Response.Cache.SetCacheability(HttpCacheability.Private);
        }
    }
}
