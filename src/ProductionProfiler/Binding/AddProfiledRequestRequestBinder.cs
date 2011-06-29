using System.Collections.Generic;
using System.Collections.Specialized;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Binding
{
    public class AddProfiledRequestRequestBinder : IAddProfiledRequestRequestBinder
    {
        private readonly List<ModelValidationError> _errors = new List<ModelValidationError>();
        
        public ProfiledRequest Bind(NameValueCollection formParams)
        {
            ProfiledRequest profiledRequest = new ProfiledRequest
            {
                Url = formParams.Get("Url"),
                ProfilingCount = formParams.Get("ProfilingCount").IsNullOrEmpty() ? (int?)null : int.Parse(formParams.Get("ProfilingCount")),
                Server = formParams.Get("Server"),
                Enabled = true
            };

            return profiledRequest;
        }

        public bool IsValid(NameValueCollection formParams)
        {
            if (string.IsNullOrEmpty(formParams.Get("Url")))
            {
                _errors.Add(new ModelValidationError
                {
                    Field = "Url",
                    Message = "Url was not supplied"
                });

                return false;
            }

            return true;
        }

        public List<ModelValidationError> Errors
        {
            get { return _errors; }
        }
    }
}
