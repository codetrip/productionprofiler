using Castle.DynamicProxy;
using ProductionProfiler.Interfaces;

namespace ProductionProfiler.IoC
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
                _requestProfiler.MethodEntry(invocation);
                invocation.Proceed();
                _requestProfiler.MethodExit();
            }
        }
    }
}
