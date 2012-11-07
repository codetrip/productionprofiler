using System;
using System.Collections.Generic;
using System.Linq;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Persistence.Entities;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.RequestTiming.Entities;
using PE = ProductionProfiler.Core.Persistence.Entities;

namespace ProductionProfiler.Persistence.Mongo
{
    public class MongoProfilerRepository : IProfilerRepository
    {
        public const string UrlToProfileDatabaseName = "UrlToProfiles";
        public const string StoredResponseDatabaseName = "profiledresponses";
        public const string UrlToProfileDataDatabaseName = "UrlToProfiledata";

        private readonly MongoConfiguration _configuration;

        public MongoProfilerRepository(MongoConfiguration configuration)
        {
            _configuration = configuration;
        }

        public PE.Page<UrlToProfile> GetUrlsToProfile(PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Page<UrlToProfile>(pagingInfo);
            }
        }

        public UrlToProfile GetUrlToProfile(string url)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Items<UrlToProfile>().FirstOrDefault(b => b.Url == url);
            }
        }

        public List<UrlToProfile> GetCurrentUrlsToProfile()
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Items<UrlToProfile>().Where(req => req.Enabled).Where(req => req.ProfilingCount == null || req.ProfilingCount > 0).ToList();
            }
        }

        public void SaveUrlToProfile(UrlToProfile urlToProfile)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Save(urlToProfile);
            }
        }

        public void DeleteUrlToProfile(string url)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<UrlToProfile>(x => x.Url == url);
            }
        }

        public ProfiledRequestData GetProfiledRequestDataById(Guid id)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Items<ProfiledRequestData>().FirstOrDefault(b => b.Id == id);
            }
        }

        public void DeleteProfiledRequestDataById(Guid id)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<ProfiledRequestData>(x => x.Id == id);
            }
        }

        public void SaveProfiledRequestData(ProfiledRequestData data)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Save(data);
            }
        }

        public void SaveResponse(ProfiledResponse response)
        {
            using (MongoSession session = MongoSession.Connect(StoredResponseDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Save(response);
            }
        }

        public ProfiledResponse GetResponseById(Guid id)
        {
            using (MongoSession session = MongoSession.Connect(StoredResponseDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Single<ProfiledResponse>(r => r.Id == id);
            }
        }

        public void DeleteResponseById(Guid id)
        {
            using (MongoSession session = MongoSession.Connect(StoredResponseDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<ProfiledResponse>(x => x.Id == id);
            }
        }

        public void DeleteResponseByUrl(string url)
        {
            using (MongoSession session = MongoSession.Connect(StoredResponseDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<ProfiledResponse>(x => x.Url == url);
            }
        }

        public void SaveTimedRequest(TimedRequest timedRequest)
        {
            using (var session = MongoSession.Connect(UrlToProfileDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Save(timedRequest);
            }
        }

        public PE.Page<TimedRequest> GetLongRequests(PagingInfo paging)
        {
            using (var session = MongoSession.Connect(UrlToProfileDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Page<TimedRequest, DateTime>(paging, orderExpression: tr => tr.RequestUtc, sortAscending: false);
            }
        }

        public void DeleteAllTimedRequests()
        {
            using (var session = MongoSession.Connect(UrlToProfileDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.DeleteAll<TimedRequest>();
            }
        }

        public PE.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewByUrl(string url, PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Page<ProfiledRequestData, DateTime, ProfiledRequestDataPreview>(
                    pagingInfo: pagingInfo,
                    whereExpression: app => app.Url == url, 
                    sortExpression: app => app.CapturedOnUtc,
                    sortAscending: false,
                    selectExpression: t => new ProfiledRequestDataPreview
                    {
                        CapturedOnUtc = t.CapturedOnUtc,
                        ElapsedMilliseconds = t.ElapsedMilliseconds,
                        Id = t.Id,
                        Server = t.Server,
                        Url = t.Url
                    });
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewBySessionId(Guid sessionId, PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Page<ProfiledRequestData, DateTime, ProfiledRequestDataPreview>(
                    pagingInfo: pagingInfo, 
                    whereExpression: app => app.SessionId == sessionId,
                    sortExpression: app => app.CapturedOnUtc,
                    sortAscending: false,
                    selectExpression: t => new ProfiledRequestDataPreview
                    {
                        CapturedOnUtc = t.CapturedOnUtc,
                        ElapsedMilliseconds = t.ElapsedMilliseconds,
                        Id = t.Id,
                        Server = t.Server,
                        Url = t.Url
                    });
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewBySessionUserId(string sessionUserId, PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Page<ProfiledRequestData, DateTime, ProfiledRequestDataPreview>(
                    pagingInfo: pagingInfo, 
                    whereExpression: app => app.SessionUserId == sessionUserId,
                    sortExpression: app => app.CapturedOnUtc,
                    sortAscending: false,
                    selectExpression: t => new ProfiledRequestDataPreview
                    {
                        CapturedOnUtc = t.CapturedOnUtc,
                        ElapsedMilliseconds = t.ElapsedMilliseconds,
                        Id = t.Id,
                        Server = t.Server,
                        Url = t.Url
                    });
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewBySamplingId(Guid samplingId, PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Page<ProfiledRequestData, DateTime, ProfiledRequestDataPreview>(
                    pagingInfo: pagingInfo,
                    whereExpression: app => app.SamplingId == samplingId,
                    sortExpression: app => app.CapturedOnUtc, 
                    sortAscending: false,
                    selectExpression: t => new ProfiledRequestDataPreview
                    {
                        CapturedOnUtc = t.CapturedOnUtc,
                        ElapsedMilliseconds = t.ElapsedMilliseconds,
                        Id = t.Id,
                        Server = t.Server,
                        Url = t.Url
                    });
            }
        }

        public PE.Page<string> GetDistinctUrlsToProfile(PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Distinct<ProfiledRequestData, string>(x => x.Url, pagingInfo, url => url.Url, true);
            }
        }

        public void DeleteProfiledRequestDataByUrl(string url)
        {
            using (MongoSession session = MongoSession.Connect(UrlToProfileDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<ProfiledRequestData>(x => x.Url == url);
            }
        }
    }
}
