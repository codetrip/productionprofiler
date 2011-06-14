using System;
using System.Linq;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Mongo;

namespace ProductionProfiler.DataAccess
{
    public class ProfiledRequestDataMongoRepository : IProfiledRequestDataRepository
    {
        private const string ProfileRequestDataDatabaseName = "profiledrequestdata";

        private readonly MongoConfiguration _configuration;

        public ProfiledRequestDataMongoRepository(MongoConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ProfiledRequestData GetById(Guid id)
        {
            using (MongoSession session = MongoSession.Connect(ProfileRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Items<ProfiledRequestData>().Where(b => b.Id == id).FirstOrDefault();
            }
        }

        public void Delete(Guid id)
        {
            using (MongoSession session = MongoSession.Connect(ProfileRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<object>(new { Id = id });
            }
        }

        public void Save(ProfiledRequestData entity)
        {
            using (MongoSession session = MongoSession.Connect(ProfileRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Save(entity);
            }
        }

        public Page<ProfiledRequestDataPreview> GetPreviewByUrl(string url, PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(ProfileRequestDataDatabaseName, _configuration.Server, _configuration.Port))
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

        public Page<string> GetDistinctUrls(PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(ProfileRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Distinct<ProfiledRequestData, string, string>("Url", pagingInfo, url => url, true);
            }
        }

        public void DeleteByUrl(string url)
        {
            using (MongoSession session = MongoSession.Connect(ProfileRequestDataDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<object>(new { Url = url });
            }
        }
    }
}
