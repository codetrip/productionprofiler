﻿
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
        }

        public static class Actions
        {
            public const string PreviewResults = "previewresults";
            public const string Results = "results";
            public const string ResultDetails = "resultsdetail";
        }

        public static class Urls
        {
            public const string Profiler = "/profiler";
            public const string ProfilerHandler = "/profiler?handler={0}";
            public const string ProfilerHandlerAction = "/profiler?handler={0}&action={1}";
            public const string ProfilerHandlerActionId = "/profiler?handler={0}&action={1}&id={2}";
            public const string ProfilerHandlerActionUrl = "/profiler?handler={0}&action={1}&url={2}";
        }

        public static class Handlers
        {
            public const string Results = "results";
            public const string ViewResponse = "response";
            public const string ViewProfiledRequests = "vpr";
            public const string AddProfiledRequest = "apr";
            public const string UpdateProfiledRequest = "upr";
            public const string DeleteProfiledRequestDataById = "dprid";
            public const string DeleteProfiledRequestDataByUrl = "dprurl";
        }
    }
}