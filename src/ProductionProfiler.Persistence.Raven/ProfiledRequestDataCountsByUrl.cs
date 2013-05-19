using System;
using System.Linq;
using ProductionProfiler.Core.Profiling.Entities;
using Raven.Client.Indexes;

namespace ProductionProfiler.Persistence.Raven
{
    public class ProfiledRequestDataCountsByUrl : AbstractIndexCreationTask<ProfiledRequestData, ProfiledRequestCount>
    {
        public ProfiledRequestDataCountsByUrl()
        {
            Map = docs => from profiledRequest in docs
                          select new
                                     {
                                         profiledRequest.Url,
                                         Count = 1,
                                         MostRecentUtc = profiledRequest.CapturedOnUtc,
                                     };

            Reduce = results => from result in results
                                group result by result.Url
                                into urls
                                select new
                                           {
                                               Url = urls.Key,
                                               Count = urls.Sum(url => url.Count),
                                               MostRecentUtc = urls.Max(url => (DateTimeOffset)url.MostRecentUtc),
                                           };
        }
    }
}