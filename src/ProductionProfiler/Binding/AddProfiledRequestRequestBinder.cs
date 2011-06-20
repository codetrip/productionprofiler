using System.Collections.Generic;
using System.Collections.Specialized;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

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
                ProfilingCount = int.Parse(formParams.Get("ProfilingCount")),
                Server = formParams.Get("Server"),
                Enabled = true
            };

            return profiledRequest;
        }

        public bool IsValid(NameValueCollection formParams)
        {
            bool valid = true;
            int profileCount;

            if (!int.TryParse(formParams.Get("ProfilingCount"), out profileCount))
            {
                _errors.Add(new ModelValidationError
                {
                    Field = "ProfilingCount",
                    Message = "ProfileCount was not supplied"
                });

                valid = false;
            }

            if (string.IsNullOrEmpty(formParams.Get("Url")))
            {
                _errors.Add(new ModelValidationError
                {
                    Field = "Url",
                    Message = "Url was not supplied"
                });

                valid = false;
            }

            return valid;
        }

        public List<ModelValidationError> Errors
        {
            get { return _errors; }
        }
    }
}
