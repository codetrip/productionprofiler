using System;
using System.Collections.Generic;
using System.Linq;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Mongo;

namespace ProductionProfiler.DataAccess
{
    public class ProfiledRequestMongoRepository : IProfiledRequestRepository
    {
        private const string UrlProfilingDatabaseName = "profiledrequests";

        private readonly MongoConfiguration _configuration;

        public ProfiledRequestMongoRepository(MongoConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ProfiledRequest GetById(string id)
        {
            using (MongoSession session = MongoSession.Connect(UrlProfilingDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Items<ProfiledRequest>().Where(b => b.Id == id).FirstOrDefault();
            }
        }

        public void Delete(string id)
        {
            using (MongoSession session = MongoSession.Connect(UrlProfilingDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<object>(new { Id = id });
            }
        }

        public void Save(ProfiledRequest entity)
        {
            using (MongoSession session = MongoSession.Connect(UrlProfilingDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Save(entity);
            }
        }

        public Page<ProfiledRequest> GetProfiledRequests(PagingInfo pagingInfo)
        {
            using (MongoSession session = MongoSession.Connect(UrlProfilingDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Page<ProfiledRequest>(pagingInfo);
            }
        }

        public IList<ProfiledRequest> GetRequestsToProfile(string serverName)
        {
            using (MongoSession session = MongoSession.Connect(UrlProfilingDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Items<ProfiledRequest>(i => i.Server == null || i.Server == serverName).ToList();
            }
        }
    }
}
