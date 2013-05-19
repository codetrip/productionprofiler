using System;
using Castle.DynamicProxy;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.IoC
{
    public interface IRequestProfilingInterceptor : IInterceptor, IDoNotWantToBeProfiled
    { }

    public class RequestProfilingInterceptor : IRequestProfilingInterceptor
    {
        private const string ExceptionCaptured = "exception_captured_by_profiler";

        public void Intercept(IInvocation invocation)
        {
            if (!ProfilerContext.Profiling)
            {
                invocation.Proceed();
            }
            else
            {
                var methodInvocation = new MethodInvocation
                {
                    Arguments = invocation.Arguments,
                    InvocationTarget = invocation.InvocationTarget,
                    TargetType = invocation.TargetType,
                    MethodName = invocation.Method.Name
                };

                ProfilerContext.Profiler.MethodEntry(methodInvocation);

                try
                {
                    invocation.Proceed();
                    methodInvocation.ReturnValue = invocation.ReturnValue;
                }
                catch (Exception e)
                {
                    //only log this exception once in the method in which it occured
                    if (!e.Data.Contains(ExceptionCaptured))
                    {
                        methodInvocation.Exception = e;
                        e.Data.Add(ExceptionCaptured, true);
                    }
                    throw;
                }
                finally
                {
                    ProfilerContext.Profiler.MethodExit(methodInvocation);
                }
            }
        }
    }
}
