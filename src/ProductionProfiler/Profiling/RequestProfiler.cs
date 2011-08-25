using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Web;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Logging;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling
{
    public class RequestProfiler : ComponentBase, IRequestProfiler
    {
        private readonly IHttpRequestDataCollector _httpRequestDataCollector;
        private readonly IHttpResponseDataCollector _httpResponseDataCollector;
        private readonly IMethodDataCollector _methodDataCollector;
        private readonly Stack<MethodData> _methodStack = new Stack<MethodData>();
        private readonly ProfilerConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly Guid _requestId;
        private ProfiledRequestData _profileData;
        private Stopwatch _watch;
        private IEnumerable<IProfilingTrigger> _coordinators;

        public RequestProfiler(IHttpRequestDataCollector httpRequestDataCollector, 
            IHttpResponseDataCollector httpResponseDataCollector, 
            IMethodDataCollector methodDataCollector,
            ILogger logger,
            ProfilerConfiguration configuration)
        {
            _methodDataCollector = methodDataCollector;
            _logger = logger;
            _httpResponseDataCollector = httpResponseDataCollector;
            _httpRequestDataCollector = httpRequestDataCollector;
            _requestId = Guid.NewGuid();
            _configuration = configuration;
        }

        public void Start(HttpContext context, IEnumerable<IProfilingTrigger> coordinators)
        {
            ProfilerContext.Profiling = true;
            _coordinators = coordinators;

            Trace("Started profiling for Url:={0}, Request Id:={1}, Server:={2}...", context.Request.RawUrl, _requestId, Environment.MachineName);

            try
            {
                _logger.Start();

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
                _profileData.RequestData.AddIfNotNull(_httpRequestDataCollector.Collect(context.Request));

                //set up our response filter to capture the full response
                //bug in asp.net 3.5 requires you to read the response Filter before you set it!
                var f = context.Response.Filter;
                context.Response.Filter = new StoreResponseFilter(context.Response.Filter);

                _watch = Stopwatch.StartNew();
            }
            catch (Exception e)
            {
                Error(e);
                Stop(context.Response);
            } 
            
            Trace("Start profiling complete");
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
                _logger.Stop();

                if(_watch != null)
                {
                    _watch.Stop();
                    _profileData.ElapsedMilliseconds = _watch.ElapsedMilliseconds;
                }

                //add data from the configured IHttpRequestDataCollector
                _profileData.ResponseData.AddIfNotNull(_httpResponseDataCollector.Collect(response));

                var responseFilter = response.Filter as IResponseFilter;
                if (responseFilter != null)
                {
                    ProfilerContext.EnqueueForPersistence(new ProfiledResponse
                    {
                        Body = responseFilter.Response,
                        Id = _requestId,
                        Url = _profileData.Url
                    });
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
                    ProfilerContext.EnqueueForPersistence(_profileData);
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

                if (_configuration.DataCollectorMappings.HasMappings())
                {
                    foreach (var collector in _configuration.DataCollectorMappings.GetMethodDataCollectorsForType(invocation.TargetType))
                    {
                        collector.Entry(invocation);
                    }
                }

                if (_configuration.DataCollectorMappings.CollectMethodDataForType(invocation.TargetType))
                {
                    method.Arguments = _methodDataCollector.GetArguments(invocation).ToList();
                }
            }
            catch(Exception e)
            {
                Error(e);
                method.Exceptions.AddException(e, _watch.ElapsedMilliseconds);
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

                //if an exception was thrown by the method, log it here
                if (invocation.Exception != null)
                {
                    currentMethod.Exceptions.AddException(invocation.Exception, _watch.ElapsedMilliseconds);
                }

                if (_configuration.DataCollectorMappings.HasMappings())
                {
                    foreach (var collector in _configuration.DataCollectorMappings.GetMethodDataCollectorsForType(invocation.TargetType))
                    {
                        collector.Exit(invocation);
                    }
                }

                if (invocation.ReturnValue != null && _configuration.DataCollectorMappings.CollectMethodDataForType(invocation.TargetType))
                {
                    currentMethod.ReturnValue = _methodDataCollector.GetReturnValue(invocation);
                }

                currentMethod.Data = invocation.MethodData;
                _logger.CurrentMethod = _methodStack.PeekIfItems();
            }
            catch (Exception e)
            {
                Error(e);

                if(currentMethod != null)
                {
                    currentMethod.Exceptions.AddException(e, currentMethod.Elapsed());
                }
            }

            Trace("Exiting method complete");
        }
    }
}
