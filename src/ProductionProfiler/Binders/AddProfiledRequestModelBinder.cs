using System.Collections.Specialized;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;

namespace ProductionProfiler.Binders
{
    public class AddProfiledRequestModelBinder : IModelBinder<ProfiledRequest>
    {
        public ProfiledRequest Bind(NameValueCollection formParams)
        {
            ProfiledRequest profiledRequest = new ProfiledRequest
            {
                Url = formParams.Get("Url"),
                ProfilingCount = int.Parse(formParams.Get("ProfilingCount")),
                Server = formParams.Get("Server")
            };

            return profiledRequest;
        }
    }
}
