using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using log4net;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Configuration;
using System.Linq;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Resources;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Profiling
{
    public class RequestProfilingCoordinator : IRequestProfilingCoordinator
    {
        private readonly IProfilerRepository _repository;
        private readonly IRequestProfiler _requestProfiler;
        private readonly ProfilerConfiguration _configuration;
        private readonly IProfilerCacheEngine _profilerCacheEngine;

        public RequestProfilingCoordinator(IRequestProfiler requestProfiler,
            IProfilerRepository repository, 
            ProfilerConfiguration configuration, 
            IProfilerCacheEngine profilerCacheEngine)
        {
            _requestProfiler = requestProfiler;
            _profilerCacheEngine = profilerCacheEngine;
            _configuration = configuration;
            _repository = repository;
        }

        public void BeginRequest(HttpContext context)
        {
            try
            {
                context.Items["Stopwatch"] = Stopwatch.StartNew();
                string currentUrl = context.Request.RawUrl.ToLowerInvariant();

                var requestsToProfile = _repository.GetRequestsToProfile(Environment.MachineName);

                if(requestsToProfile != null && requestsToProfile.Count > 0)
                {
                    var requestToProfile = requestsToProfile.FirstOrDefault(req => req.ProfilingCount > 0 && Regex.IsMatch(currentUrl, string.Format("^{0}$", req.Url)));

                    if (requestToProfile != null)
                    {
                        requestToProfile.ProfilingCount--;

                        if (requestToProfile.ProfilingCount <= 0)
                        {
                            requestToProfile.ProfilingCount = 0;
                        }

                        _repository.SaveProfiledRequest(requestToProfile);

                        //indicate we are profiling the current request
                        RequestProfilerContext.Current.StartProfiling();

                        _requestProfiler.StartProfiling(context);
                    }
                }
            }
            catch (Exception e)
            {
                RequestProfilerContext.Current.StopProfiling();

                if(_configuration.Log4NetEnabled)
                {
                    foreach (var log in LogManager.GetCurrentLoggers())
                        log.Error(e);
                }
            }
        }

        public void EndRequest(HttpContext context)
        {
            try
            {
                if(_configuration.MonitoringEnabled)
                {
                    Stopwatch stopwatch = context.Items["Stopwatch"] as Stopwatch;

                    if (stopwatch != null)
                    {
                        stopwatch.Stop();

                        long maxRequestLength = context.Request.HttpMethod.ToLowerInvariant() == Constants.HttpMethods.Get
                            ? _configuration.GetRequestThreshold
                            : _configuration.PostRequestThreshold;

                        //if the request took over maxRequestLength and the profiler was not enabled for this request flag the URL for analysis
                        if (stopwatch.ElapsedMilliseconds >= maxRequestLength && !RequestProfilerContext.Current.ProfilingCurrentRequest())
                        {
                            _profilerCacheEngine.Remove(Constants.Handlers.ViewProfiledRequests, true);
                            _repository.SaveProfiledRequestWhenNotFound(new ProfiledRequest
                            {
                                Url = context.Request.RawUrl.ToLowerInvariant(),
                                ProfiledOnUtc = DateTime.UtcNow,
                                Server = Environment.MachineName,
                                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                                ProfilingCount = 0, //indicates we should not be monitoring this URL until its been validated via the admin console
                                HttpMethod = context.Request.HttpMethod,
                                Enabled = false
                            });
                        }
                    } 
                }

                //if the IRequestProfiler was running for this request we need to persist the captured data into Mongo via nservicebus
                if (RequestProfilerContext.Current.ProfilingCurrentRequest())
                {
                    RequestProfilerContext.Current.StopProfiling();

                    _profilerCacheEngine.Remove(Constants.Actions.Results, true);
                    _profilerCacheEngine.Remove("{0}-{1}".FormatWith(Constants.Actions.PreviewResults, context.Request.RawUrl.ToLowerInvariant()), true);
                    _repository.SaveProfiledRequestData(_requestProfiler.StopProfiling(context.Response));
                }
            }
            catch (Exception e)
            {
                RequestProfilerContext.Current.StopProfiling();

                if (_configuration.Log4NetEnabled)
                {
                    foreach (var log in LogManager.GetCurrentLoggers())
                        log.Error(e);
                }
            }
        }
    }
}
