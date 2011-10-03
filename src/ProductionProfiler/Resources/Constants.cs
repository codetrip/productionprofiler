
namespace ProductionProfiler.Core.Resources
{
    public static class Constants
    {
        public const string RequestProfileContextKey = "RequestProfiler";
        public const string ProfilingAppender = "ProfilingAppender";
        public const string Stopwatch = "Stopwatch";

        public static class HttpMethods
        {
            public const string Get = "get";
        }

        public static class RequestEncoding
        {
            public const string GZip = "gzip";
        }

        public static class CacheKeys
        {
            /// <summary>
            /// The cache key used to hold the requests (urls effectively) that should currently be profiled
            /// </summary>
            public const string CurrentRequestsToProfile = "profiler->current-requests-to-profile";
        }

        public static class HttpHeaders
        {
            public const string AcceptEncoding = "Accept-Encoding";
            public const string ContentEncoding = "Content-Encoding";
            public const string IfModifiedSince = "If-Modified-Since";
            public const string ContentType = "Content-Type";
        }

        public static class Querystring
        {
            public const string Handler = "handler";
            public const string Resource = "resource";
            public const string ContentType = "contenttype";
            public const string Action = "action";
            public const string Url = "url";
            public const string Id = "id";
            public const string PageNumber = "pgno";
            public const string PageSize = "pgsz";
            public const string ConfigKey = "key";
            public const string ConfigValue = "value";
        }

        public static class Actions
        {
            public const string PreviewResults = "previewresults";
            public const string Results = "results";
            public const string ResultDetails = "resultsdetail";
            public const string ViewConfigSettings = "viewcfg";
            public const string SetConfigSettings = "setcfg";
            public const string ViewLongRequests = "viewlongrequests";
        }

        public static class Urls
        {
            public const string Profiler = "/profiler";
            public const string ProfilerHandler = "/profiler?handler={0}";
            public const string ProfilerHandlerAction = "/profiler?handler={0}&action={1}";
            public const string ProfilerHandlerActionId = "/profiler?handler={0}&action={1}&id={2}";
            public const string ProfilerHandlerActionUrl = "/profiler?handler={0}&action={1}&url={2}";
            public const string ProfilerHandlerConfiguration = "/profiler?handler=cfg&action=viewcfg";
        }

        public static class Handlers
        {
            public const string Results = "results";
            public const string ViewResponse = "response";
            public const string ViewUrlToProfiles = "vpr";
            public const string AddUrlToProfile = "apr";
            public const string UpdateUrlToProfile = "upr";
            public const string DeleteUrlToProfileDataById = "dprid";
            public const string DeleteUrlToProfileDataByUrl = "dprurl";
            public const string ConfigurationOverride = "cfg";
            public const string LongRequests = "longrequests";
            public const string ClearLongRequests = "clearlongrequests";
        }
    }
}
