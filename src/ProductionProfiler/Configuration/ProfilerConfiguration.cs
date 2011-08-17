using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Configuration
{
    public class ProfilerConfiguration : IDoNotWantToBeProfiled
    {
        internal bool CaptureExceptions { get; set; }
        internal bool CaptureResponse { get; set; }
        internal Func<HttpContext, Stream> ResponseFilter { get; set; }
        internal MethodDataCollectorMappings MethodDataCollectorMappings { get; set; }
        internal Action<Exception> ReportException { get; set; }

        internal Func<HttpRequest, bool> RequestFilter { private get; set; }
        internal Func<HttpContext, bool> AuthoriseManagement { private get; set; }
        internal Func<string, bool> AuthoriseSession { private get; set; }

        public ProfilerConfiguration()
        {
            MethodDataCollectorMappings = new MethodDataCollectorMappings();
        }

        public IEnumerable<IProfilingCoordinator> GetCoordinators(HttpContext context)
        {
            return ProfilerContext.Container.ResolveAll<IProfilingCoordinator>().Where(pm => pm.ShouldProfile(context)); ;
        }

        public bool AuthorisedForManagement(HttpContext context)
        {
            return AuthoriseManagement == null || AuthoriseManagement(context);
        }

        public bool AuthorisedForSession(string sessionUserId)
        {
            return AuthoriseSession == null || AuthoriseSession(sessionUserId);
        }

        public bool ShouldProfileRequest(HttpRequest request)
        {
            return !request.RawUrl.Contains("/profiler") && (RequestFilter == null || (RequestFilter != null && RequestFilter(request)));
        }
    }
}