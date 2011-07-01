using Castle.DynamicProxy;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.IoC
{
    public interface IRequestProfilingInterceptor : IInterceptor, IDoNotWantToBeProfiled
    { }

    public class RequestProfilingInterceptor : IRequestProfilingInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            if (!ProfilerContext.Current.ProfilingCurrentRequest())
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

                ProfilerContext.Current.Profiler.MethodEntry(methodInvocation);

                invocation.Proceed();

                methodInvocation.ReturnValue = invocation.ReturnValue;

                ProfilerContext.Current.Profiler.MethodExit(methodInvocation);
            }
        }
    }
}
