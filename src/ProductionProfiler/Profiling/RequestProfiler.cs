using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Web;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Log4Net;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling
{
    public class RequestProfiler : IRequestProfiler
    {
        private readonly ProfilerConfiguration _configuration;
        private readonly IHttpRequestDataCollector _httpRequestDataCollector;
        private readonly IHttpResponseDataCollector _httpResponseDataCollector;
        private readonly IProfilerRepository _repository;
        private readonly int _threadId;
        private ProfiledRequestData _profileData;
        private ProfiledMethodData _currentMethod;
        private Stopwatch _watch;

        public RequestProfiler(ProfilerConfiguration configuration, 
            IHttpRequestDataCollector httpRequestDataCollector, 
            IHttpResponseDataCollector httpResponseDataCollector, 
            IProfilerRepository repository)
        {
            _configuration = configuration;
            _repository = repository;
            _httpResponseDataCollector = httpResponseDataCollector;
            _httpRequestDataCollector = httpRequestDataCollector;
            _threadId = Thread.CurrentThread.ManagedThreadId;
            RequestId = Guid.NewGuid();
        }

        public bool InitialisedForRequest { get; set; }
        public Guid RequestId { get; set; }

        public void StartProfiling(HttpContext context)
        {
            InitialisedForRequest = true;

            if (_configuration.Log4NetEnabled)
            {
                //add the logging event handler for this profiler instance
                _configuration.ProfilingAppender.AppendLoggingEvent += ProfilingAppenderAppendLoggingEvent;
            }

            _profileData = new ProfiledRequestData
            {
                Url = context.Request.RawUrl.ToLowerInvariant(),
                CapturedOnUtc = DateTime.UtcNow,
                Server = Environment.MachineName,
                ClientIpAddress = context.Request.ClientIpAddress(),
                Ajax = context.Request.IsAjaxRequest(),
                Id = RequestId,
                ProfilerErrors = RequestProfilerContext.Current.PersistentProfilerErrors.ToList()
            };

            //add data from the configured IHttpRequestDataCollector
            _profileData.RequestData.AddRangeIfNotNull(_httpRequestDataCollector.Collect(context.Request));

            if (_configuration.CaptureExceptions)
            {
                AppDomain.CurrentDomain.FirstChanceException += CaptureException;
            }

            if (_configuration.CaptureResponse)
            {
                //bug in asp.net 3.5 requires you to read the response Filter before you set it!
                var f = context.Response.Filter;
                context.Response.Filter = _configuration.GetResponseFilter(context);
                _profileData.CapturedResponse = true;
            }

            _watch = Stopwatch.StartNew();
        }

        private void CaptureException(object sender, FirstChanceExceptionEventArgs e)
        {
            //we check the thread id to check its not an exception from some other web request.
            //pretty sure this isn't a problem....
            if (_currentMethod != null && _threadId == Thread.CurrentThread.ManagedThreadId)
            {
                _currentMethod.Exceptions.Add(new ThrownException
                {
                    Message = e.Exception.Format(true, true),
                    Milliseconds = _watch.ElapsedMilliseconds,
                    Type = e.Exception.GetType().FullName
                });
            }
        }

        private void ProfilingAppenderAppendLoggingEvent(object sender, AppendLoggingEventEventArgs e)
        {
            if (_currentMethod != null)
            {
                _currentMethod.Messages.Add(e.LoggingEvent.ToLogMessage(_currentMethod.Elapsed()));
            }  
        }

        public ProfiledRequestData StopProfiling(HttpResponse response)
        {
            _watch.Stop();
            _profileData.ElapsedMilliseconds = _watch.ElapsedMilliseconds;

            //add data from the configured IHttpRequestDataCollector
            _profileData.ResponseData.AddRangeIfNotNull(_httpResponseDataCollector.Collect(response));

            if (_configuration.Log4NetEnabled)
            {
                //remove the logging event handler for this profiler instance
                _configuration.ProfilingAppender.AppendLoggingEvent -= ProfilingAppenderAppendLoggingEvent;
            }

            if (_configuration.CaptureExceptions)
                AppDomain.CurrentDomain.FirstChanceException -= CaptureException;

            if(_configuration.CaptureResponse)
            {
                var responseFilter = response.Filter as IResponseFilter;
                if(responseFilter != null)
                {
                    _repository.SaveResponse(new StoredResponse
                    {
                        Body = responseFilter.Response,
                        Id = RequestId,
                        Url = _profileData.Url
                    });
                }
            }

            return _profileData;
        }

        public void ProfilerError(string message)
        {
            _profileData.ProfilerErrors.Add(new ProfilerError
            {
                ErrorAtMilliseconds = _watch.ElapsedMilliseconds, 
                Message = message, 
                Type = ProfilerErrorType.Runtime
            });
        }

        public void MethodEntry(MethodInvocation invocation)
        {
            ProfiledMethodData method = new ProfiledMethodData
            {
                MethodName = string.Format("{0}.{1}", invocation.TargetType.FullName, invocation.MethodName)
            };

            if (_currentMethod == null)
            {
                _profileData.Methods.Add(method);
            }
            else
            {
                _currentMethod.Methods.Add(method);
                method.SetParentMethod(_currentMethod);
            }

            _currentMethod = method;

            foreach (var collector in _configuration.MethodDataCollectorMappings.GetMethodDataCollectorsForType(invocation.TargetType, RequestProfilerContext.Current.Container))
            {
                collector.Entry(invocation);
            }

            _currentMethod.StartedAtMilliseconds = _watch.ElapsedMilliseconds;
            _currentMethod.Start();
        }

        public void MethodExit(MethodInvocation invocation)
        {
            _currentMethod.StoppedAtMilliseconds = _watch.ElapsedMilliseconds;
            _currentMethod.ElapsedMilliseconds = _currentMethod.Stop();

            foreach (var collector in _configuration.MethodDataCollectorMappings.GetMethodDataCollectorsForType(invocation.TargetType, RequestProfilerContext.Current.Container))
            {
                collector.Exit(invocation);
            }

            _currentMethod.Data = invocation.MethodData;
            _currentMethod = _currentMethod.GetParentMethod();
        }
    }
}
