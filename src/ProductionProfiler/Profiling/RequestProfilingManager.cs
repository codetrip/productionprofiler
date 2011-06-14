using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using ProductionProfiler.Interfaces;

namespace ProductionProfiler.Profiling
{
    public class RequestProfilingManager : IRequestProfilingManager
    {
        private readonly IRequestProfiler _requestProfiler;

        public RequestProfilingManager(IRequestProfiler requestProfiler)
        {
            _requestProfiler = requestProfiler;
        }

        public void BeginRequest(HttpContext context)
        {
            context.Items["Stopwatch"] = Stopwatch.StartNew();

            //var state = _getPerformanceMonitoringStateQuery.Invoke(new GetPerformanceMonitoringStateRequest());

            //if (state != null && state.UrlsToMonitor != null)
            //{
            //    string currentUrl = context.Request.RawUrl.ToLowerInvariant();
            //    var urlToMonitor = state.UrlsToMonitor.FirstOrDefault(u => Regex.IsMatch(currentUrl, u));

            //    if (urlToMonitor != null)
            //    {
            //        var url = _urlProfilingRepository.GetById(urlToMonitor);

            //        if (url != null && url.MonitoringCount.HasValue)
            //        {
            //            url.MonitoringCount--;

            //            //if we have reached the end of the number of times we are monitoring this URL
            //            //then remove it from the UrlsToMonitor collection
            //            if (url.MonitoringCount <= 0)
            //            {
            //                url.MonitoringCount = null;
            //                state.UrlsToMonitor.Remove(urlToMonitor);
            //                _setPerformanceMonitoringStateCommand.Invoke(new SetPerformanceMonitoringStateRequest
            //                {
            //                    State = state
            //                });
            //            }

            //            _urlProfilingRepository.Save(url);
            //            _requestProfiler.StartProfiling(context.Request);
            //        }
            //    }
            //}
        }

        public void EndRequest(HttpContext context)
        {
            //try
            //{
            //    Stopwatch stopwatch = context.Items["Stopwatch"] as Stopwatch;

            //    if (stopwatch != null)
            //    {
            //        stopwatch.Stop();

            //        long maxRequestLength = context.Request.HttpMethod.ToLowerInvariant() == WebConstants.HttpMethods.Get
            //            ? _commonConfig.HttpGetMaxRequestMilliseconds
            //            : _commonConfig.HttpPostMaxRequestMilliseconds;

            //        //if the request took over maxRequestLength and the profiler was not enabled for this request flag the URL for analysis
            //        if (stopwatch.ElapsedMilliseconds >= maxRequestLength && !_requestProfiler.InitialisedForRequest)
            //        {
            //            _urlProfilingRepository.Save(new UrlProfilingInfo
            //            {
            //                Url = context.Request.RawUrl.ToLowerInvariant(),
            //                ProfiledOnUtc = DateTime.UtcNow,
            //                Server = Environment.MachineName,
            //                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
            //                MonitoringCount = null, //indicates we should not be monitoring this URL until its been validated through admin site,
            //                HttpMethod = context.Request.HttpMethod
            //            });
            //        }
            //    }

            //    //if the IRequestProfiler was running for this request we need to persist the captured data into Mongo via nservicebus
            //    if (_requestProfiler.InitialisedForRequest)
            //    {
            //        // _serviceBus.Publish(_requestProfiler.StopProfiling());
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //Error(ex);
            //}
        }
    }
}
