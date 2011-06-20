using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Web;
using log4net.Core;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Log4Net;
using System.Linq;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Profiling
{
    public class RequestProfiler : IRequestProfiler
    {
        private ProfiledRequestData _profileData;
        private ProfiledMethodData _currentMethod;
        private Stopwatch _watch;
        private readonly int _threadId;
        private readonly ProfilerConfiguration _configuration;
        private readonly IMethodEntryDataCollector _methodEntryDataCollector;
        private readonly IMethodExitDataCollector _methodExitDataCollector;
        private readonly IHttpRequestDataCollector _httpRequestDataCollector;
        private readonly IHttpResponseDataCollector _httpResponseDataCollector;

        public RequestProfiler(ProfilerConfiguration configuration, 
            IMethodEntryDataCollector methodEntryDataCollector, 
            IMethodExitDataCollector methodExitDataCollector, 
            IHttpRequestDataCollector httpRequestDataCollector, 
            IHttpResponseDataCollector httpResponseDataCollector)
        {
            _configuration = configuration;
            _httpResponseDataCollector = httpResponseDataCollector;
            _httpRequestDataCollector = httpRequestDataCollector;
            _methodExitDataCollector = methodExitDataCollector;
            _methodEntryDataCollector = methodEntryDataCollector;
            _threadId = Thread.CurrentThread.ManagedThreadId;
            RequestId = Guid.NewGuid();
        }

        public bool InitialisedForRequest { get; set; }
        public Guid RequestId { get; set; }

        public void StartProfiling(HttpRequest request)
        {
            if(HttpContext.Current != null)
            {
                HttpContext.Current.Items[Constants.RequestProfileContextKey] = true;

                InitialisedForRequest = true;

                if (_configuration.Log4NetEnabled)
                {
                    //add the logging event handler for this profiler instance
                    _configuration.ProfilingAppender.AppendLoggingEvent += ProfilingAppenderAppendLoggingEvent;
                }

                _profileData = new ProfiledRequestData
                {
                    Url = request.RawUrl.ToLowerInvariant(),
                    CapturedOnUtc = DateTime.UtcNow,
                    Server = Environment.MachineName,
                    ClientIpAddress = request.ClientIpAddress(),
                    UserAgent = request.UserAgent,
                    Ajax = request.IsAjaxRequest(),
                    Id = RequestId,
                    ProfilerErrors = RequestProfilerContext.Current.PersistentProfilerErrors.ToList()
                };

                //add data from the configured IHttpRequestDataCollector
                var data = _httpRequestDataCollector.Collect(request);

                if (data != null)
                    _profileData.CollectedData.AddRange(data);

                _watch = Stopwatch.StartNew();

                if (_configuration.CaptureExceptions)
                {
                    AppDomain.CurrentDomain.FirstChanceException += CaptureException;
                }
            }
        }

        private void CaptureException(object sender, FirstChanceExceptionEventArgs e)
        {
            //we check the thread id to check its not an exception from some other web request.
            //pretty sure this isn't a problem....
            if (_currentMethod != null && _threadId == Thread.CurrentThread.ManagedThreadId)
            {
                _currentMethod.Exceptions.Add(new ThrownException
                {
                    CallStack = e.Exception.StackTrace,
                    Message = e.Exception.Message,
                    Milliseconds = _watch.ElapsedMilliseconds,
                    Type = e.Exception.GetType().FullName
                });
            }
        }

        private void ProfilingAppenderAppendLoggingEvent(object sender, AppendLoggingEventEventArgs e)
        {
            if (_currentMethod != null)
            {
                if (e.LoggingEvent.Level >= Level.Error)
                {
                    _currentMethod.ErrorInMethod = true;
                }

                _currentMethod.Messages.Add(e.LoggingEvent.ToLogMessage(_currentMethod.Elapsed()));
            }  
        }

        public ProfiledRequestData StopProfiling(HttpResponse response)
        {
            _watch.Stop();
            _profileData.ElapsedMilliseconds = _watch.ElapsedMilliseconds;

            //add data from the configured IHttpRequestDataCollector
            var data = _httpResponseDataCollector.Collect(response);

            if (data != null)
                _profileData.CollectedData.AddRange(data);

            if (_configuration.Log4NetEnabled)
            {
                //remove the logging event handler for this profiler instance
                _configuration.ProfilingAppender.AppendLoggingEvent -= ProfilingAppenderAppendLoggingEvent;
            }

            if (_configuration.CaptureExceptions)
                AppDomain.CurrentDomain.FirstChanceException -= CaptureException;

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

            var data = _methodEntryDataCollector.Collect(invocation);

            if (data != null)
                _currentMethod.CollectedData.AddRange(data);

            _currentMethod.StartedAtMilliseconds = _watch.ElapsedMilliseconds;
            _currentMethod.Start();
        }

        public void MethodExit(MethodInvocation invocation)
        {
            var data = _methodExitDataCollector.Collect(invocation);

            if (data != null)
                _currentMethod.CollectedData.AddRange(data);

            _currentMethod.StoppedAtMilliseconds = _watch.ElapsedMilliseconds;
            _currentMethod.ElapsedMilliseconds = _currentMethod.Stop();
            _currentMethod = _currentMethod.GetParentMethod();
        }
    }
}
