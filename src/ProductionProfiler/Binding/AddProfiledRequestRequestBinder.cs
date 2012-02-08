using System.Collections.Generic;
using System.Collections.Specialized;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Binding
{
    public class AddUrlToProfileRequestBinder : IAddUrlToProfileRequestBinder
    {
        private readonly List<ModelValidationError> _errors = new List<ModelValidationError>();
        
        public UrlToProfile Bind(NameValueCollection formParams)
        {
            UrlToProfile urlToProfile = new UrlToProfile
            {
                Url = formParams.Get("Url"),
                ProfilingCount = formParams.Get("ProfilingCount").IsNullOrEmpty() ? (int?)null : int.Parse(formParams.Get("ProfilingCount")),
                Server = formParams.Get("Server"),
                ThresholdForRecordingMs = formParams.Get("ThresholdForRecordingMs").IsNullOrEmpty() ? (int?)null : int.Parse(formParams.Get("ThresholdForRecordingMs")),
                Enabled = true
            };

            return urlToProfile;
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
