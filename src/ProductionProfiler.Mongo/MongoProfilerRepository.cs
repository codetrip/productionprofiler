using System;
using System.Collections.Generic;
using System.Linq;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;

namespace ProductionProfiler.Mongo
{
    public class MongoProfilerRepository : IProfilerRepository
    {
        private const string ProfiledRequestDatabaseName = "profiledrequests";
        private const string ProfiledRequestDataDatabaseName = "profiledrequestdata";

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

        public IList<ProfiledRequest> GetRequestsToProfile(string serverName)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Items<ProfiledRequest>(i => i.Server == null || i.Server == serverName).Where(i => i.Enabled).ToList();
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

        public void Delete<TTemplate>(TTemplate template)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<ProfiledRequestData, TTemplate>(template);
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
                session.Delete<object>(new { RequestId = id });
            }
        }

        public void SaveProfiledRequestData(ProfiledRequestData entity)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Save(entity);
            }
        }

        public Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewByUrl(string url, PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(ProfiledRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Page<ProfiledRequestData, DateTime, ProfiledRequestDataPreview>(
                    pagingInfo,
                    app => app.Url == url,
                    app => app.CapturedOnUtc,
                    false,
                    t => new ProfiledRequestDataPreview
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
                session.Delete<object>(new { Url = url });
            }
        }
    }
}
