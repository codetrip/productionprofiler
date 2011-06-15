using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;

namespace ProductionProfiler.Binders
{
    public class UpdateProfiledRequestModelBinder : IModelBinder<IEnumerable<ProfiledRequestUpdate>>
    {
        public IEnumerable<ProfiledRequestUpdate> Bind(NameValueCollection formParams)
        {
            var profiledRequest = new List<ProfiledRequestUpdate>();

            foreach(var key in formParams.AllKeys)
            {
                AddParam(key, formParams, profiledRequest);
            }

            return profiledRequest;
        }

        private static void AddParam(string key, NameValueCollection formParams, List<ProfiledRequestUpdate> requests)
        {
            var match = Regex.Match(key, @"(.*)\[([0-9]+)\](.*)");

            if(match.Success)
            {
                int index = int.Parse(match.Groups[2].Value);
                string property = match.Groups[3].Value;

                if (requests.Count <= index)
                    requests.Add(new ProfiledRequestUpdate());

                var request = requests[index];

                switch (property)
                {
                    case "Url":
                        request.Url = formParams[key];
                        break;
                    case "ProfilingCount":
                        request.ProfilingCount = string.IsNullOrEmpty(formParams[key]) ? (int?)null : int.Parse(formParams[key]);
                        break;
                    case "Server":
                        request.Server = formParams[key];
                        break;
                    case "Ignore":
                        request.Ignore = formParams[key].Contains("true");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
