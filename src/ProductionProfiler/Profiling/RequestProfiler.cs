using System;
using System.Collections.Generic;
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
        private readonly IMethodInputOutputDataCollector _methodInputOutputDataCollector;
        private readonly Stack<MethodData> _methodStack = new Stack<MethodData>(); 
        private readonly ILogger _logger;
        private readonly int _threadId;
        private ProfiledRequestData _profileData;
        private Stopwatch _watch;

        public RequestProfiler(ProfilerConfiguration configuration, 
            IHttpRequestDataCollector httpRequestDataCollector, 
            IHttpResponseDataCollector httpResponseDataCollector, 
            IProfilerRepository repository, 
            IMethodInputOutputDataCollector methodInputOutputDataCollector,
            ILogger logger)
        {
            _configuration = configuration;
            _methodInputOutputDataCollector = methodInputOutputDataCollector;
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
                Id = RequestId
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
                context.Response.Filter = _configuration.ResponseFilter(context);
                _profileData.CapturedResponse = true;
            }

            _watch = Stopwatch.StartNew();
        }

        private void CaptureException(object sender, FirstChanceExceptionEventArgs e)
        {
            try
            {
                var currentMethod = _methodStack.PeekIfItems();

                //we check the thread id to check its not an exception from some other web request.
                //pretty sure this isn't a problem....
                if (currentMethod != null && _threadId == Thread.CurrentThread.ManagedThreadId && !currentMethod.Exceptions.Any(ex => ex.Type == e.Exception.GetType().FullName))
                {
                    currentMethod.Exceptions.Add(new ThrownException
                    {
                        Message = e.Exception.Format(true, true),
                        Milliseconds = _watch.ElapsedMilliseconds,
                        Type = e.Exception.GetType().FullName
                    });
                }
            }
            catch
            {
                //get a stack overflow if an exceptions is thrown by this method
            }
        }

        public void StopProfiling(HttpResponse response)
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

            _repository.SaveProfiledRequestData(_profileData);
        }

        public void MethodEntry(MethodInvocation invocation)
        {
            MethodData method = new MethodData
            {
                MethodName = string.Format("{0}.{1}", invocation.TargetType.FullName, invocation.MethodName)
            };

            var currentMethod = _methodStack.PeekIfItems();

            if (currentMethod == null)
            {
                currentMethod = method;
                _profileData.Methods.Add(currentMethod);
            }
            else
            {
                currentMethod.Methods.Add(method);
            }

            try
            {
                if (_configuration.MethodDataCollectorMappings.AnyMappedTypes())
                {
                    foreach (var collector in ProfilerContext.Current.GetMethodDataCollectorsForType(invocation.TargetType))
                    {
                        collector.Entry(invocation);
                    }
                }

                if(_configuration.MethodDataCollectorMappings.ShouldCollectInputOutputDataForType(invocation.TargetType))
                {
                    method.Arguments = _methodInputOutputDataCollector.GetArguments(invocation).ToList();
                }
            }
            catch (Exception e)
            {
                ProfilerContext.Current.Exception(e);
            }

            method.StartedAtMilliseconds = _watch.ElapsedMilliseconds;
            method.Start();
            _logger.CurrentMethod = method;
            _methodStack.Push(method);
        }

        public void MethodExit(MethodInvocation invocation)
        {
            var currentMethod = _methodStack.Pop();
            currentMethod.StoppedAtMilliseconds = _watch.ElapsedMilliseconds;
            currentMethod.ElapsedMilliseconds = currentMethod.Stop();

            try
            {
                if (_configuration.MethodDataCollectorMappings.AnyMappedTypes())
                {
                    foreach (var collector in ProfilerContext.Current.GetMethodDataCollectorsForType(invocation.TargetType))
                    {
                        collector.Exit(invocation);
                    }
                }

                if (_configuration.MethodDataCollectorMappings.ShouldCollectInputOutputDataForType(invocation.TargetType))
                {
                    currentMethod.ReturnValue = _methodInputOutputDataCollector.GetReturnValue(invocation);
                }
            }
            catch (Exception e)
            {
                ProfilerContext.Current.Exception(e);
            }

            currentMethod.Data = invocation.MethodData;
            _logger.CurrentMethod = _methodStack.PeekIfItems();
        }
    }
}
