using System;
using System.Collections.Generic;
using System.Linq;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Persistence.Entities;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Persistence.Mongo
{
    public class MongoProfilerRepository : IProfilerRepository
    {
        public const string ProfiledRequestDatabaseName = "profiledrequests";
        public const string StoredResponseDatabaseName = "profiledresponses";
        public const string ProfiledRequestDataDatabaseName = "profiledrequestdata";

        private readonly MongoConfiguration _configuration;

        public MongoProfilerRepository(MongoConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Page<ProfiledRequest> GetProfiledRequests(PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Page<ProfiledRequest>(pagingInfo);
            }
        }

        public ProfiledRequest GetProfiledRequestByUrl(string url)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Items<ProfiledRequest>().Where(b => b.Url == url).FirstOrDefault();
            }
        }

        public List<ProfiledRequest> GetCurrentRequestsToProfile()
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Items<ProfiledRequest>().Where(req => req.Enabled).Where(req => req.ProfilingCount == null || req.ProfilingCount > 0).ToList();
            }
        }

        public void SaveProfiledRequestWhenNotFound(ProfiledRequest profiledRequest)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDatabaseName, _configuration.Server, _configuration.Port))
            {
                var request = session.Items<ProfiledRequest>().Where(b => b.Url == profiledRequest.Url).Select(r => r.Url).FirstOrDefault();

                if (request == null)
                    session.Save(profiledRequest);
            }
        }

        public void SaveProfiledRequest(ProfiledRequest profiledRequest)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Save(profiledRequest);
            }
        }

        public void DeleteProfiledRequest(string url)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<ProfiledRequest, object>(new { Url = url });
            }
        }

        public ProfiledRequestData GetProfiledRequestDataById(Guid id)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Items<ProfiledRequestData>().Where(b => b.Id == id).FirstOrDefault();
            }
        }

        public void DeleteProfiledRequestDataById(Guid id)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<ProfiledRequestData, object>(new { Id = id });
            }
        }

        public void SaveProfiledRequestData(ProfiledRequestData entity)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Save(entity);
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
                session.Delete<ProfiledResponse, object>(new { Id = id });
            }
        }

        public void DeleteResponseByUrl(string url)
        {
            using (MongoSession session = MongoSession.Connect(StoredResponseDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<ProfiledResponse, object>(new { Url = url });
            }
        }

        public Page<ProfiledRequestPreview> GetProfiledRequestDataPreviewByUrl(string url, PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Page<ProfiledRequestData, DateTime, ProfiledRequestPreview>(
                    pagingInfo,
                    app => app.Url == url,
                    app => app.CapturedOnUtc,
                    false,
                    t => new ProfiledRequestPreview
                    {
                        CapturedOnUtc = t.CapturedOnUtc,
                        ElapsedMilliseconds = t.ElapsedMilliseconds,
                        Id = t.Id,
                        Server = t.Server,
                        Url = t.Url
                    });
            }
        }

        public Page<string> GetDistinctProfiledRequestUrls(PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Distinct<ProfiledRequestData, string, string>("Url", pagingInfo, url => url, true);
            }
        }

        public void DeleteProfiledRequestDataByUrl(string url)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<ProfiledRequestData, object>(new { Url = url });
            }
        }
    }
}
