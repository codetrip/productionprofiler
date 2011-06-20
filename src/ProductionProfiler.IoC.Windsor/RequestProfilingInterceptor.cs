using Castle.DynamicProxy;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.IoC.Windsor
{
    public interface IRequestProfilingInterceptor : IInterceptor
    { }

    public class RequestProfilingInterceptor : IRequestProfilingInterceptor
    {
        private readonly IRequestProfiler _requestProfiler;

        public RequestProfilingInterceptor(IRequestProfiler requestProfiler)
        {
            _requestProfiler = requestProfiler;
        }

        public void Intercept(IInvocation invocation)
        {
            if (!_requestProfiler.InitialisedForRequest)
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

                _requestProfiler.MethodEntry(methodInvocation);

                invocation.Proceed();

                methodInvocation.ReturnValue = invocation.ReturnValue;

                _requestProfiler.MethodExit(methodInvocation);
            }
        }
    }
}
