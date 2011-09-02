
using System.Collections.Generic;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Resources;
using ProductionProfiler.Core.Extensions;
using System.Linq;

namespace ProductionProfiler.Core.Handlers
{
    public class ConfigurationOverrideHandler : RequestHandlerBase
    {
        private readonly ProfilerConfiguration _configuration;

        public ConfigurationOverrideHandler(ProfilerConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            JsonResponse response = new JsonResponse
            {
                Data = _configuration.Settings
            };

            if(requestInfo.Action.Equals(Constants.Actions.ViewConfigSettings))
            {
                response.Success = true;
                return response;
            }

            foreach (string key in requestInfo.Form.AllKeys.Where(k => k.StartsWith("cfg-")))
            {
                string configKey = key.Split(new [] {'-'})[1];
                if (_configuration.Settings.ContainsKey(configKey))
                {
                    _configuration.Settings[configKey] = requestInfo.Form[key];
                }
            }
            
            response.Success = true;
            response.Redirect = string.Format("{0}&s=1", Constants.Urls.ProfilerHandlerConfiguration);
            return response;
        }
    }
}
