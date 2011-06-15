
using System.Collections.Specialized;

namespace ProductionProfiler.Interfaces
{
    public interface IModelBinder<out T>
    {
        T Bind(NameValueCollection formParams);
    }
}
