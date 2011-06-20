using Castle.DynamicProxy;
using ProductionProfiler.Core.Interfaces;

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
                _requestProfiler.MethodEntry(string.Format("{0}.{1}", invocation.TargetType.FullName, invocation.Method.Name));
                invocation.Proceed();
                _requestProfiler.MethodExit();
            }
        }
    }
}
