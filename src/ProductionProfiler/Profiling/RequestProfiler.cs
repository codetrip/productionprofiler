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
    public class RequestProfiler : ComponentBase, IRequestProfiler
    {
        private readonly IHttpRequestDataCollector _httpRequestDataCollector;
        private readonly IHttpResponseDataCollector _httpResponseDataCollector;
        private readonly IProfilerRepository _repository;
        private readonly IMethodInputOutputDataCollector _methodInputOutputDataCollector;
        private readonly Stack<MethodData> _methodStack = new Stack<MethodData>();
        private readonly ProfilerConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly int _threadId;
        private readonly Guid _requestId;
        private ProfiledRequestData _profileData;
        private Stopwatch _watch;
        private IEnumerable<IProfilingCoordinator> _coordinators;

        public RequestProfiler(IHttpRequestDataCollector httpRequestDataCollector, 
            IHttpResponseDataCollector httpResponseDataCollector, 
            IProfilerRepository repository, 
            IMethodInputOutputDataCollector methodInputOutputDataCollector,
            ILogger logger,
            ProfilerConfiguration configuration)
        {
            _methodInputOutputDataCollector = methodInputOutputDataCollector;
            _logger = logger;
            _repository = repository;
            _httpResponseDataCollector = httpResponseDataCollector;
            _httpRequestDataCollector = httpRequestDataCollector;
            _threadId = Thread.CurrentThread.ManagedThreadId;
            _requestId = Guid.NewGuid();
            _configuration = configuration;
        }

        public void Start(HttpContext context, IEnumerable<IProfilingCoordinator> coordinators)
        {
            ProfilerContext.Profiling = true;
            _coordinators = coordinators;

            Trace("Started profiling for Url:={0}, Request Id:={1}, Server:={2}...", context.Request.RawUrl, _requestId, Environment.MachineName);

            try
            {
                _logger.StartProfiling();

                _profileData = new ProfiledRequestData
                {
                    Url = context.Request.RawUrl.ToLowerInvariant(),
                    CapturedOnUtc = DateTime.UtcNow,
                    Server = Environment.MachineName,
                    ClientIpAddress = context.Request.ClientIpAddress(),
                    Ajax = context.Request.IsAjaxRequest(),
                    Id = _requestId
                };

                //add data from the configured IHttpRequestDataCollector
                _profileData.RequestData.AddRangeIfNotNull(_httpRequestDataCollector.Collect(context.Request));

                if (_configuration.CaptureExceptions)
                {
                    Trace("...Profiler configured to capture all exceptions");
                    AppDomain.CurrentDomain.FirstChanceException += CaptureException;
                }

                if (_configuration.CaptureResponse)
                {
                    //bug in asp.net 3.5 requires you to read the response Filter before you set it!
                    var f = context.Response.Filter;
                    context.Response.Filter = _configuration.ResponseFilter(context);
                    _profileData.CapturedResponse = true;
                    Trace("...Profiler configured to capture response, adding response filter of type:={0}", context.Response.Filter.GetType());
                }

                _watch = Stopwatch.StartNew();
            }
            catch (Exception e)
            {
                Error(e);
                Stop(context.Response);
            } 
            
            Trace("Start profiling complete");
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

        public void Stop(HttpResponse response)
        {
            if (!ProfilerContext.Profiling)
            {
                Trace("Stop invoked, but profiler not running, returning...");
                return;
            }

            ProfilerContext.Profiling = false;
            Trace("Stop Profiling RequestId:={0}...", _requestId);

            try
            {
                _logger.StopProfiling();

                if(_watch != null)
                {
                    _watch.Stop();
                    _profileData.ElapsedMilliseconds = _watch.ElapsedMilliseconds;
                }

                //add data from the configured IHttpRequestDataCollector
                _profileData.ResponseData.AddRangeIfNotNull(_httpResponseDataCollector.Collect(response));

                if (_configuration.CaptureExceptions)
                    AppDomain.CurrentDomain.FirstChanceException -= CaptureException;

                if (_configuration.CaptureResponse)
                {
                    var responseFilter = response.Filter as IResponseFilter;
                    if (responseFilter != null)
                    {
                        _repository.SaveResponse(new ProfiledResponse
                        {
                            Body = responseFilter.Response,
                            Id = _requestId,
                            Url = _profileData.Url
                        });
                    }
                }

                //allow the coordinators that have been enabled for this request to augment to profile data before we save it.
                foreach (var coordinator in _coordinators)
                {
                    coordinator.AugmentProfiledRequestData(_profileData);
                }
            }
            catch (Exception e)
            {
                Error(e);
            }
            finally
            {
                try
                {
                    Trace("...Attempting to persist profile data");
                    _repository.SaveProfiledRequestData(_profileData);
                    Trace("...Success");
                }
                catch (Exception ex)
                {
                    Error(ex);
                }
            }

            Trace("Stop profiling complete.");
        }


        public void MethodEntry(MethodInvocation invocation)
        {
            MethodData method = new MethodData
            {
                MethodName = string.Format("{0}.{1}", invocation.TargetType.FullName, invocation.MethodName)
            };

            Trace("Entering method:={0}...", method.MethodName);

            try
            {
                var currentMethod = _methodStack.PeekIfItems();

                if (currentMethod == null)
                    _profileData.Methods.Add(method);
                else
                    currentMethod.Methods.Add(method);

                if (_configuration.MethodDataCollectorMappings.AnyMappedTypes())
                {
                    foreach (var collector in _configuration.MethodDataCollectorMappings.GetMethodDataCollectorsForType(invocation.TargetType))
                    {
                        collector.Entry(invocation);
                    }
                }

                if (_configuration.MethodDataCollectorMappings.ShouldCollectInputOutputDataForType(invocation.TargetType))
                {
                    method.Arguments = _methodInputOutputDataCollector.GetArguments(invocation).ToList();
                }
            }
            catch(Exception e)
            {
                Error(e);

                method.Exceptions.Add(new ThrownException
                {
                    Message = e.Format(),
                    Milliseconds = _watch.ElapsedMilliseconds,
                    Type = e.GetType().ToString()
                });
            }
            finally
            {
                method.Start(_watch.ElapsedMilliseconds);
                _logger.CurrentMethod = method;
                _methodStack.Push(method);
            }

            Trace("Entering method complete");
        }

        public void MethodExit(MethodInvocation invocation)
        {
            Trace("Exiting method:={0}...", string.Format("{0}.{1}", invocation.TargetType.FullName, invocation.MethodName));

            MethodData currentMethod = null;

            try
            {
                currentMethod = _methodStack.Pop();
                currentMethod.Stop( _watch.ElapsedMilliseconds);

                if (_configuration.MethodDataCollectorMappings.AnyMappedTypes())
                {
                    foreach (var collector in _configuration.MethodDataCollectorMappings.GetMethodDataCollectorsForType(invocation.TargetType))
                    {
                        collector.Exit(invocation);
                    }
                }

                if (_configuration.MethodDataCollectorMappings.ShouldCollectInputOutputDataForType(invocation.TargetType))
                {
                    currentMethod.ReturnValue = _methodInputOutputDataCollector.GetReturnValue(invocation);
                }

                currentMethod.Data = invocation.MethodData;
                _logger.CurrentMethod = _methodStack.PeekIfItems();
            }
            catch (Exception e)
            {
                Error(e);

                if(currentMethod != null)
                {
                    currentMethod.Exceptions.Add(new ThrownException
                    {
                        Message = e.Format(),
                        Milliseconds = currentMethod.Elapsed(),
                        Type = e.GetType().ToString()
                    });
                }
            }

            Trace("Exiting method complete");
        }
    }
}
