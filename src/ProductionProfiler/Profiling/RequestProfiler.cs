using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Web;
using log4net.Core;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Interfaces;
using ProductionProfiler.Core.Interfaces.Entities;
using ProductionProfiler.Core.Interfaces.Resources;
using ProductionProfiler.Core.Log4Net;
using System.Linq;

namespace ProductionProfiler.Core.Profiling
{
    public class RequestProfiler : IRequestProfiler
    {
        private ProfiledRequestData _profileData;
        private ProfiledMethodData _currentMethod;
        private Stopwatch _watch;
        private readonly ProfilerConfiguration _configuration;
        private readonly int _threadId;

        public RequestProfiler(ProfilerConfiguration configuration)
        {
            _configuration = configuration;
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
                    Methods = new List<ProfiledMethodData>(),
                    Server = Environment.MachineName,
                    ClientIpAddress = request.ClientIpAddress(),
                    UserAgent = request.UserAgent,
                    Ajax = request.IsAjaxRequest(),
                    Id = RequestId,
                    ProfilerErrors = RequestProfilerContext.Current.PersistentProfilerErrors.ToList(),
                };

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
                _currentMethod.Exceptions.Add(new ThrownException()
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

        public ProfiledRequestData StopProfiling()
        {
            _watch.Stop();
            _profileData.ElapsedMilliseconds = _watch.ElapsedMilliseconds;

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

        public void MethodEntry(string methodName)
        {
            ProfiledMethodData method = new ProfiledMethodData
            {
                MethodName = methodName
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
            _currentMethod.StartedAtMilliseconds = _watch.ElapsedMilliseconds;
            _currentMethod.Start();
        }

        public void MethodExit()
        {
            _currentMethod.StoppedAtMilliseconds = _watch.ElapsedMilliseconds;
            _currentMethod.ElapsedMilliseconds = _currentMethod.Stop();
            _currentMethod = _currentMethod.GetParentMethod();
        }
    }
}
