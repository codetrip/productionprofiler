using System;
using System.Collections.Generic;
using ProductionProfiler.Core.Persistence.Entities;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Persistence
{
    public interface IProfilerRepository
    {
        /// <summary>
        /// Returns a page of Profiled requests ordered by date captured descending
        /// </summary>
        /// <param name="pagingInfo"></param>
        /// <returns></returns>
        Page<ProfiledRequest> GetProfiledRequests(PagingInfo pagingInfo);
        /// <summary>
        /// Returns a ProfiledRequest instance matching the specified URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        ProfiledRequest GetProfiledRequestByUrl(string url);
        /// <summary>
        /// Returns a list of all ProfiledRequests that are active for the specified server. Active means, Enabled and with a ProfilingCount > 0
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        IList<ProfiledRequest> GetRequestsToProfile(string serverName);
        /// <summary>
        /// Saves ProfiledRequest if an only if an instance with the ProfiledRequest.Url does not already exist
        /// </summary>
        /// <param name="profiledRequest"></param>
        void SaveProfiledRequestWhenNotFound(ProfiledRequest profiledRequest);
        /// <summary>
        /// Saves the ProfiledRequest instance to the data store (should overwrite if an instance with the same URL already exists)
        /// </summary>
        /// <param name="profiledRequest"></param>
        void SaveProfiledRequest(ProfiledRequest profiledRequest);
        /// <summary>
        /// Deletes the ProfiledRequest instance for the specified URL
        /// </summary>
        /// <param name="url"></param>
        void DeleteProfiledRequest(string url);

        /// <summary>
        /// Returns a preview of all profiled request data capruted for the specified URL
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pagingInfo"></param>
        /// <returns></returns>
        Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewByUrl(string url, PagingInfo pagingInfo);
        /// <summary>
        /// Returns a ProfiledRequestData instance matching the specified Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ProfiledRequestData GetProfiledRequestDataById(Guid id);
        /// <summary>
        /// Returns a distinct page of URL's from profiled requests
        /// </summary>
        /// <param name="pagingInfo"></param>
        /// <returns></returns>
        Page<string> GetDistinctProfiledRequestUrls(PagingInfo pagingInfo);
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
        /// Saves the StoredResponse instance to the data store
        /// </summary>
        /// <param name="response"></param>
        void SaveResponse(StoredResponse response);
        /// <summary>
        /// Retrieves a StoredResponse instance from the data store
        /// </summary>
        /// <param name="id"></param>
        StoredResponse GetResponseById(Guid id);
        /// <summary>
        /// Deletes a StoredResponse instance from the data store
        /// </summary>
        /// <param name="id"></param>
        void DeleteResponseById(Guid id);
        /// <summary>
        /// Deletes a StoredResponse instance from the data store using the URL
        /// </summary>
        /// <param name="url"></param>
        void DeleteResponseByUrl(string url);
    }
}
