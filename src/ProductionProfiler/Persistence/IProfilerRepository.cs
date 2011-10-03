using System;
using System.Collections.Generic;
using ProductionProfiler.Core.Persistence.Entities;
using ProductionProfiler.Core.RequestTiming;
using PE = ProductionProfiler.Core.Persistence.Entities;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Persistence
{
    public interface IProfilerRepository : IDoNotWantToBeProfiled
    {
        /// <summary>
        /// Returns a page of URL's to profile ordered by URL
        /// </summary>
        /// <param name="pagingInfo"></param>
        /// <returns></returns>
        PE.Page<UrlToProfile> GetUrlsToProfile(PagingInfo pagingInfo);
        /// <summary>
        /// Returns a UrlToProfile instance matching the specified URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        UrlToProfile GetUrlToProfile(string url);
        /// <summary>
        /// Returns a list of all UrlToProfiles that are active. Active means, Enabled and with a ProfilingCount > 0
        /// </summary>
        /// <returns></returns>
        List<UrlToProfile> GetCurrentUrlsToProfile();
        /// <summary>
        /// Saves the UrlToProfile instance to the data store (should overwrite if an instance with the same URL already exists)
        /// </summary>
        /// <param name="urlToProfile"></param>
        void SaveUrlToProfile(UrlToProfile urlToProfile);
        /// <summary>
        /// Deletes the UrlToProfile instance for the specified URL
        /// </summary>
        /// <param name="url"></param>
        void DeleteUrlToProfile(string url);

        PE.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewByUrl(string url, PagingInfo pagingInfo);
        PE.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewBySessionId(Guid sessionId, PagingInfo pagingInfo);
        PE.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewBySessionUserId(string sessionUserId, PagingInfo pagingInfo);
        PE.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewBySamplingId(Guid samplingId, PagingInfo pagingInfo);

        ProfiledRequestData GetProfiledRequestDataById(Guid id);
        /// <summary>
        /// Returns a distinct page of URL's from profiled requests
        /// </summary>
        /// <param name="pagingInfo"></param>
        /// <returns></returns>
        PE.Page<string> GetDistinctUrlsToProfile(PagingInfo pagingInfo);
        /// <summary>
        /// Deletes all profiled request data captured for the specified URL
        /// </summary>
        /// <param name="url"></param>
        void DeleteProfiledRequestDataByUrl(string url);
        /// <summary>
        /// Deletes profiled request data by Id
        /// </summary>
        /// <param name="id"></param>
        void DeleteProfiledRequestDataById(Guid id);
        /// <summary>
        /// Saves the ProfiledRequestData instance to the data store
        /// </summary>
        /// <param name="profiledRequestData"></param>
        void SaveProfiledRequestData(ProfiledRequestData profiledRequestData);

        /// <summary>
        /// Saves the ProfiledResponse instance to the data store
        /// </summary>
        /// <param name="response"></param>
        void SaveResponse(ProfiledResponse response);
        /// <summary>
        /// Retrieves a ProfiledResponse instance from the data store
        /// </summary>
        /// <param name="id"></param>
        ProfiledResponse GetResponseById(Guid id);
        /// <summary>
        /// Deletes a ProfiledResponse instance from the data store
        /// </summary>
        /// <param name="id"></param>
        void DeleteResponseById(Guid id);
        /// <summary>
        /// Deletes a ProfiledResponse instance from the data store using the URL
        /// </summary>
        /// <param name="url"></param>
        void DeleteResponseByUrl(string url);

        void SaveTimedRequest(TimedRequest timedRequest);
        PE.Page<TimedRequest> GetLongRequests(PagingInfo paging);
        void DeleteAllTimedRequests();
    }
}
