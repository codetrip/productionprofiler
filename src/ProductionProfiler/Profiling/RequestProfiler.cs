using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Web;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Logging;
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
        private readonly ILogger _logger;
        private readonly int _threadId;
        private ProfiledRequestData _profileData;
        private MethodData _currentMethod;
        private Stopwatch _watch;

        public RequestProfiler(ProfilerConfiguration configuration, 
            IHttpRequestDataCollector httpRequestDataCollector, 
            IHttpResponseDataCollector httpResponseDataCollector, 
            IProfilerRepository repository, 
            ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
            _repository = repository;
            _httpResponseDataCollector = httpResponseDataCollector;
            _httpRequestDataCollector = httpRequestDataCollector;
            _threadId = Thread.CurrentThread.ManagedThreadId;
            RequestId = Guid.NewGuid();
        }

        public Guid RequestId { get; set; }

        public void StartProfiling(HttpContext context)
        {
            _logger.StartProfiling();

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
            if (_currentMethod != null && _threadId == Thread.CurrentThread.ManagedThreadId && !_currentMethod.Exceptions.Any(ex => ex.Type == e.Exception.GetType().FullName))
            {
                _currentMethod.Exceptions.Add(new ThrownException
                {
                    Message = e.Exception.Format(true, true),
                    Milliseconds = _watch.ElapsedMilliseconds,
                    Type = e.Exception.GetType().FullName
                });
            }
        }

        public ProfiledRequestData StopProfiling(HttpResponse response)
        {
            _logger.StartProfiling();
            _watch.Stop();
            _profileData.ElapsedMilliseconds = _watch.ElapsedMilliseconds;

            //add data from the configured IHttpRequestDataCollector
            _profileData.ResponseData.AddRangeIfNotNull(_httpResponseDataCollector.Collect(response));

            if (_configuration.CaptureExceptions)
                AppDomain.CurrentDomain.FirstChanceException -= CaptureException;

            if(_configuration.CaptureResponse)
            {
                var responseFilter = response.Filter as IResponseFilter;
                if(responseFilter != null)
                {
                    _repository.SaveResponse(new ProfiledResponse
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
            MethodData method = new MethodData
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
            _logger.CurrentMethod = _currentMethod;
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
            _logger.CurrentMethod = _currentMethod;
        }
    }
}
