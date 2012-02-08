using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Handlers.Entities;

namespace ProductionProfiler.Core.Binding
{
    public class UpdateUrlToProfileRequestBinder : IUpdateUrlToProfileRequestBinder
    {
        private readonly List<ModelValidationError> _errors = new List<ModelValidationError>();

        public UrlToProfileUpdateModel Bind(NameValueCollection formParams)
        {
            var UrlToProfile = new UrlToProfileUpdateModel
            {
                Delete = formParams.AllKeys.Where(k => k == "Delete").FirstOrDefault() != null,
                Url = formParams.Get("Url")
            };

            if (UrlToProfile.Delete)
                return UrlToProfile;

            UrlToProfile.Server = formParams.Get("Server");
            UrlToProfile.ProfilingCount = formParams.Get("ProfilingCount").IsNullOrEmpty() ? (int?)null : int.Parse(formParams.Get("ProfilingCount"));
            UrlToProfile.Enabled = formParams.Get("Enabled").Contains("true");
            UrlToProfile.ThresholdForRecordingMs = formParams.Get("ThresholdForRecordingMs").IsNullOrEmpty()
                                                       ? (int?) null
                                                       : int.Parse(formParams.Get("ThresholdForRecordingMs"));

            return UrlToProfile;
        }

        public bool IsValid(NameValueCollection formParams)
        {
            bool valid = true;

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

