using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Coordinators
{
    public class UrlCoordinator : ComponentBase, IProfilingCoordinator
    {
        private readonly IProfilerRepository _repository;
        private readonly IProfilerCacheEngine _profilerCacheEngine;

        public UrlCoordinator(IProfilerRepository repository, IProfilerCacheEngine profilerCacheEngine)
        {
            _profilerCacheEngine = profilerCacheEngine;
            _repository = repository;
        }

        public bool ShouldProfile(HttpContext context)
        {
            Trace("Determining whether to run the URL profiling coordinator...");

            List<UrlToProfile> urlsToProfile = _profilerCacheEngine.Get<List<UrlToProfile>>(Constants.CacheKeys.CurrentRequestsToProfile);

            if (urlsToProfile == null)
            {
                Trace("...No requests to profile found in the cache, retrieving from repository");
                //if we did not find the requests to profile for this machine
                urlsToProfile = _repository.GetCurrentUrlsToProfile() ?? new List<UrlToProfile>();
                _profilerCacheEngine.Put(urlsToProfile, Constants.CacheKeys.CurrentRequestsToProfile);
            }

            if (urlsToProfile.Count > 0)
            {
                string currentUrl = context.Request.RawUrl.ToLowerInvariant();

                Trace("...Found {0} URL's to profile, looking to see if any match the current URL {1}", urlsToProfile.Count, currentUrl);

                //profile this request if the server name is empty (indicates profile on any machine)
                //and the current request URL matches the UrlToProfile.Url regex
                var requestToProfile = urlsToProfile.FirstOrDefault(
                    req => (req.Server.IsNullOrEmpty() || req.Server == Environment.MachineName) && Regex.IsMatch(currentUrl, string.Format("^{0}$", req.Url)));

                if (requestToProfile != null)
                {
                    Trace("...Found a match, decrementing the profiling count");

                    //if there is a ProfilingCount then decrement it, update the UrlToProfile and invalidate the cache
                    if (requestToProfile.ProfilingCount.HasValue)
                    {
                        requestToProfile.ProfilingCount--;

                        if (requestToProfile.ProfilingCount <= 0)
                            requestToProfile.ProfilingCount = 0;

                        //store the updated UrlToProfile instance
                        _repository.SaveUrlToProfile(requestToProfile);

                        //remove the CurrentRequestsToProfile cache item as it has been updated here
                        _profilerCacheEngine.Remove(Constants.CacheKeys.CurrentRequestsToProfile);

                        Trace("...Profiling count decremented, {0} profiles left.", requestToProfile.ProfilingCount);
                    }

                    Trace("Complete");
                    return true;
                }
            }
            else
            {
                Trace("...No URL's to profile, will not profile the request for the UrlCoordinator");
            }

            Trace("Complete");
            return false;
        }

        /// <summary>
        /// Nothing to do here
        /// </summary>
        /// <param name="data"></param>
        public void AugmentProfiledRequestData(ProfiledRequestData data)
        {}
    }
}
