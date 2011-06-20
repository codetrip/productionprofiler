using System.Collections.Generic;
using System.Collections.Specialized;
using ProductionProfiler.Core.Handlers.Entities;

namespace ProductionProfiler.Core.Binding
{
    public interface IRequestBinder<out T>
    {
        T Bind(NameValueCollection formParams);
        bool IsValid(NameValueCollection formParams);
        List<ModelValidationError> Errors { get; }
    }
}
