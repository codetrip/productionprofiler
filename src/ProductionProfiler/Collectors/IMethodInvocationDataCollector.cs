using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Collectors
{
    public interface IMethodInvocationDataCollector : IDoNotWantToBeProfiled
    {
        /// <summary>
        /// Invoked prior to the method being called
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        void Entry(MethodInvocation invocation);
        /// <summary>
        /// Invoked after the method has been called, if there is a return type for 
        /// the method the invocation.ReturnValue will be set with this value.
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        void Exit(MethodInvocation invocation);
    }
}
