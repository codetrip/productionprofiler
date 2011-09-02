using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Configuration
{
    public class ProfilerConfiguration : IDoNotWantToBeProfiled
    {
        internal DataCollectorMappings DataCollectorMappings { get; set; }
        internal Action<Exception> ReportException { get; set; }
        internal SamplingConfiguration SamplingConfiguration { get; set; }
        internal Dictionary<string, string> Settings { get; set; }
        internal Func<HttpRequest, bool> RequestFilter { private get; set; }
        internal Func<HttpContext, bool> AuthoriseManagement { private get; set; }
        internal Func<string, bool> AuthoriseSession { private get; set; }

        public ProfilerConfiguration()
        {
            DataCollectorMappings = new DataCollectorMappings();
            Settings = new Dictionary<string, string>();
        }

        public IEnumerable<IProfilingTrigger> GetCoordinators(HttpContext context)
        {
            return ProfilerContext.Container.ResolveAll<IProfilingTrigger>().Where(pm => pm.TriggerProfiling(context));
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
            return Settings.ContainsKey(SettingKeys.ProfilerEnabled) &&
                   Settings[SettingKeys.ProfilerEnabled] == "true" && 
                   !request.RawUrl.Contains("/profiler") && 
                   (RequestFilter == null || (RequestFilter != null && RequestFilter(request)));
        }

        public static class SettingKeys
        {
            public const string ProfilerEnabled = "profiler.enabled";
            public const string UrlTriggerEnabled = "url.trigger.enabled";
            public const string SessionTriggerEnabled = "session.trigger.enabled";
            public const string SamplingTriggerEnabled = "sampling.trigger.enabled";
        }
    }
}