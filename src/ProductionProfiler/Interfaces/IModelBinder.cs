
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ProductionProfiler.Core.Interfaces
{
    public interface IModelBinder<out T>
    {
        T Bind(NameValueCollection formParams);
        bool IsValid(NameValueCollection formParams);
        List<ModelValidationError> Errors { get; }
    }
}
