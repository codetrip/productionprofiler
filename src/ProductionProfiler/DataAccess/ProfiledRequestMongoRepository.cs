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

        public ProfiledRequest GetById(string url)
        {
            using (MongoSession session = MongoSession.Connect(UrlProfilingDatabaseName, _configuration.Server, _configuration.Port))
            {
                return session.Items<ProfiledRequest>().Where(b => b.Url == url).FirstOrDefault();
            }
        }

        public void Delete<TTemplate>(TTemplate template)
        {
            using (MongoSession session = MongoSession.Connect(UrlProfilingDatabaseName, _configuration.Server, _configuration.Port))
            {
                session.Delete<ProfiledRequest, TTemplate>(template);
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
                return session.Items<ProfiledRequest>(i => (i.Server == null || i.Server == serverName) && i.Enabled).ToList();
            }
        }

        public void Update(ProfiledRequest profiledRequest)
        {
            using (MongoSession session = MongoSession.Connect(UrlProfilingDatabaseName, _configuration.Server, _configuration.Port))
            {
                var request = session.Items<ProfiledRequest>().Where(b => b.Url == profiledRequest.Url).Select(r => r.Url).FirstOrDefault();

                if(request == null)
                    session.Save(profiledRequest);
            }
        }
    }
}
