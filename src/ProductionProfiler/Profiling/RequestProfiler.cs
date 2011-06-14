using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using Castle.DynamicProxy;
using ProductionProfiler.Configuration;
using ProductionProfiler.Extensions;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;
using ProductionProfiler.Log4Net;

namespace ProductionProfiler.Profiling
{
    public class RequestProfiler : IRequestProfiler
    {
        private ProfiledRequestInfo _profileData;
        private ProfiledMethodInfo _currentMethod;
        private Stopwatch _watch;
        private readonly ProfilerConfiguration _configuration;

        public RequestProfiler(ProfilerConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool InitialisedForRequest { get; set; }
        public Guid RequestId { get; set; }

        public bool AcceptingAuditOutput
        {
            get { return InitialisedForRequest && _currentMethod != null; }
        }

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

                _profileData = new ProfiledRequestInfo
                {
                    Url = request.RawUrl.ToLowerInvariant(),
                    CapturedOnUtc = DateTime.UtcNow,
                    Methods = new List<ProfiledMethodInfo>(),
                    Server = Environment.MachineName,
                    ClientIpAddress = request.ClientIpAddress(),
                    UserAgent = request.UserAgent,
                    Ajax = request.IsAjaxRequest(),
                    RequestId = RequestId
                };

                _watch = Stopwatch.StartNew();
            }
        }

        private void ProfilingAppenderAppendLoggingEvent(object sender, AppendLoggingEventEventArgs e)
        {
            if (_currentMethod != null)
                _currentMethod.LogMessages.Add(e.LoggingEvent.ToLogMessage(_currentMethod.Elapsed()));
        }

        public ProfiledRequestInfo StopProfiling()
        {
            _watch.Stop();
            _profileData.ElapsedMilliseconds = _watch.ElapsedMilliseconds;

            if (_configuration.Log4NetEnabled)
            {
                //remove the logging event handler for this profiler instance
                _configuration.ProfilingAppender.AppendLoggingEvent -= ProfilingAppenderAppendLoggingEvent;
            }

            return _profileData;
        }

        public void MethodEntry(IInvocation invocation)
        {
            ProfiledMethodInfo method = new ProfiledMethodInfo
            {
                MethodName = string.Format("{0}.{1}", invocation.TargetType.FullName, invocation.Method.Name)
            };

            if (_currentMethod == null)
            {
                _profileData.Methods.Add(method);
            }
            else
            {
                _currentMethod.InnerMethods.Add(method);
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
