using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Configuration
{
    public sealed class ProfilerConfiguration : IDoNotWantToBeProfiled
    {
        internal DataCollectorMappings DataCollectorMappings { get; set; }
        internal Action<Exception> ReportException { get; set; }
        internal Dictionary<string, string> Settings { get; set; }
        internal Func<HttpRequest, bool> RequestFilter { private get; set; }
        internal Func<HttpContext, bool> AuthoriseManagement { private get; set; }
        internal Func<string, bool> AuthoriseSession { private get; set; }

        public bool TimeAllRequests
        {
            get { return Settings.ContainsKey(SettingKeys.TimeAllRequests) && Settings[SettingKeys.TimeAllRequests] == "true";}
        }

        public long LongRequestThresholdMs { get { return long.Parse(Settings[SettingKeys.LongRequestThresholdMs]); } }

        public ProfilerConfiguration()
        {
            DataCollectorMappings = new DataCollectorMappings();
            Settings = new Dictionary<string, string>();
        }

        public IEnumerable<IProfilingTrigger> GetTriggers(HttpContext context)
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

        public bool ShouldTimeRequest(HttpRequest request)
        {
            return TimeAllRequests && ShouldProfileRequest(request);
        }

        public ProfiledRequestData GetCurrentProfiledData()
        {
            //put a function on here to allow for locations other than HttpContext to have the data.
            var context =
                HttpContext.Current != null 
                ? HttpContext.Current.Items[Constants.RequestProfileContextHttpContextItemKey] as RequestProfileContext
                : null;

            return context != null ? context.ProfiledRequestData : null;
        }

        public static class SettingKeys
        {
            public const string TimeAllRequests = "timer.on";
            public const string ProfilerEnabled = "profiler.enabled";
            public const string UrlTriggerEnabled = "url.trigger.enabled";
            public const string SessionTriggerEnabled = "session.trigger.enabled";
            public const string SamplingTriggerEnabled = "sampling.trigger.enabled";
            public const string SamplingPeriod = "sampling.period";
            public const string SamplingFrequency = "sampling.frequency";
            public const string LongRequestThresholdMs = "longrequest.threshold";
        }
    }
}