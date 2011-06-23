using System.Collections.Generic;
using System.Collections.Specialized;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Binding
{
    public interface IRequestBinder<out T> : IDoNotWantToBeProfiled
    {
        T Bind(NameValueCollection formParams);
        bool IsValid(NameValueCollection formParams);
        List<ModelValidationError> Errors { get; }
    }
}
