using System;
using System.Collections.Generic;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Persistence.Entities;
using ProductionProfiler.Core.Profiling.Entities;
using Raven.Client.Document;
using Raven.Client.Linq;
using System.Linq;

namespace ProductionProfiler.Persistence.Raven
{
    public class RavenProfilerRepository : IProfilerRepository
    {
        public const string UrlToProfileIndexName = "UrlToProfileIndex";
        public const string UrlToProfileDataIndexName = "UrlToProfileDataIndex";
        public const string ProfiledResponseIndexName = "ProfiledResponseIndex";

        private readonly DocumentStore _database;

        public RavenProfilerRepository(DocumentStore database)
        {
            _database = database;
        }

        public Core.Persistence.Entities.Page<UrlToProfile> GetUrlsToProfile(PagingInfo pagingInfo)
        {
            using (var session = _database.OpenSession())
            {
                var documentQuery = session.Advanced.LuceneQuery<UrlToProfile>(UrlToProfileIndexName)
                    .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize)
                    .WaitForNonStaleResults(new TimeSpan(0, 0, 3));

                return new Core.Persistence.Entities.Page<UrlToProfile>(
                    documentQuery.ToList(), 
                    new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, documentQuery.QueryResult.TotalResults));
            }
        }

        public UrlToProfile GetUrlToProfile(string url)
        {
            using (var session = _database.OpenSession())
            {
                return session.Query<UrlToProfile>(UrlToProfileIndexName).Where(r => r.Url == url).FirstOrDefault();
            }
        }

        public List<UrlToProfile> GetCurrentUrlsToProfile()
        {
            using (var session = _database.OpenSession())
            {
                return session.Query<UrlToProfile>(UrlToProfileIndexName)
                    .Where(req => req.Enabled)
                    .Where(req => req.ProfilingCount == null || req.ProfilingCount > 0)
                    .ToList();
            }
        }

        public void SaveUrlToProfile(UrlToProfile urlToProfile)
        {
            using (var session = _database.OpenSession())
            {
                // HRB: removed this code as it's causing problems ("Error: Attempted to associated a different object with id"),
                // and it's not necessary - Raven will simply overwrite the stored profile, which is what we want.
                
                /*
                var request = session.Query<UrlToProfile>(UrlToProfileIndexName)
                    .Where(r => r.Url == UrlToProfile.Url)
                    .FirstOrDefault();

                //if one exists delete it and replace with the new incoming request
                if (request != null)
                {
                    session.Delete(request);
                }
                */

                session.Store(urlToProfile);
                session.SaveChanges();
            }
        }

        public void DeleteUrlToProfile(string url)
        {
            using (var session = _database.OpenSession())
            {
                var item = session.Query<UrlToProfile>(UrlToProfileIndexName).Where(r => r.Url == url).FirstOrDefault();

                if (item != null)
                {
                    session.Delete(item);
                    session.SaveChanges();
                }
            }
        }

        public Core.Persistence.Entities.Page<string> GetDistinctUrlsToProfile(PagingInfo pagingInfo)
        {
            using (var session = _database.OpenSession())
            {
                var documentQuery = session.Query<ProfiledRequestData>(UrlToProfileDataIndexName)
                    .Distinct()
                    .Select(r => r.Url);

                var count = documentQuery.Count();
                var items = documentQuery
                    .Skip((pagingInfo.PageNumber - 1)*pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize)
                    .ToList();

                return new Core.Persistence.Entities.Page<string>(
                    items,
                    new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
            }
        }

        public ProfiledRequestData GetProfiledRequestDataById(Guid id)
        {
            using (var session = _database.OpenSession())
            {
                return session.Query<ProfiledRequestData>(UrlToProfileDataIndexName).Where(r => r.Id == id).FirstOrDefault();
            }
        }

        public void DeleteProfiledRequestDataById(Guid id)
        {
            using (var session = _database.OpenSession())
            {
                var item = session.Query<ProfiledRequestData>(UrlToProfileDataIndexName).Where(r => r.Id == id).FirstOrDefault();

                if (item != null)
                {
                    session.Delete(item);
                    session.SaveChanges();
                }
            }
        }

        public void SaveProfiledRequestData(ProfiledRequestData data)
        {
            using (var session = _database.OpenSession())
            {
                session.Store(data);
                session.SaveChanges();
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewByUrl(string url, PagingInfo pagingInfo)
        {
            using (var session = _database.OpenSession())
            {
                var linqQuery = session.Query<ProfiledRequestData>(UrlToProfileDataIndexName).Where(r => r.Url == url);
                return DoGetPreview(linqQuery, pagingInfo);
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewBySessionId(Guid sessionId, PagingInfo pagingInfo)
        {
            using (var session = _database.OpenSession())
            {
                var linqQuery = session.Query<ProfiledRequestData>(UrlToProfileDataIndexName).Where(r => r.SessionId == sessionId);
                return DoGetPreview(linqQuery, pagingInfo);
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewBySessionUserId(string sessionUserId, PagingInfo pagingInfo)
        {
            using (var session = _database.OpenSession())
            {
                var linqQuery = session.Query<ProfiledRequestData>(UrlToProfileDataIndexName).Where(r => r.SessionUserId == sessionUserId);
                return DoGetPreview(linqQuery, pagingInfo);
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewBySamplingId(Guid samplingId, PagingInfo pagingInfo)
        {
            using (var session = _database.OpenSession())
            {
                var linqQuery = session.Query<ProfiledRequestData>(UrlToProfileDataIndexName).Where(r => r.SamplingId == samplingId);
                return DoGetPreview(linqQuery, pagingInfo);
            }
        }

        private Core.Persistence.Entities.Page<ProfiledRequestDataPreview> DoGetPreview(IRavenQueryable<ProfiledRequestData> linqQuery, PagingInfo pagingInfo)
        {
            var pagination = new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, linqQuery.Count());
            var results = linqQuery.OrderByDescending(r => r.CapturedOnUtc)
                .Select(r => new ProfiledRequestDataPreview
                {
                    CapturedOnUtc = r.CapturedOnUtc,
                    ElapsedMilliseconds = r.ElapsedMilliseconds,
                    Id = r.Id,
                    Server = r.Server,
                    Url = r.Url
                }).Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize).Take(pagingInfo.PageSize);

            return new Core.Persistence.Entities.Page<ProfiledRequestDataPreview>(results.ToList(), pagination);
        }

        public void DeleteProfiledRequestDataByUrl(string url)
        {
            using (var session = _database.OpenSession())
            {
                foreach (var document in session.Query<ProfiledRequestData>(UrlToProfileDataIndexName).Where(r => r.Url == url))
                {
                    session.Delete(document);
                }
                session.SaveChanges();
            }
        }

        public void SaveResponse(ProfiledResponse response)
        {
            using (var session = _database.OpenSession())
            {
                session.Store(response);
                session.SaveChanges();
            }
        }

        public ProfiledResponse GetResponseById(Guid id)
        {
            using (var session = _database.OpenSession())
            {
                return session.Query<ProfiledResponse>(ProfiledResponseIndexName).Where(r => r.Id == id).FirstOrDefault();
            }
        }

        public void DeleteResponseById(Guid id)
        {
            using (var session = _database.OpenSession())
            {
                var item = session.Query<ProfiledResponse>(ProfiledResponseIndexName).Where(r => r.Id == id).FirstOrDefault();

                if (item != null)
                {
                    session.Delete(item);
                    session.SaveChanges();
                }
            }
        }

        public void DeleteResponseByUrl(string url)
        {
            using (var session = _database.OpenSession())
            {
                foreach (var document in session.Query<ProfiledResponse>(ProfiledResponseIndexName).Where(r => r.Url == url))
                {
                    session.Delete(document);
                }
                session.SaveChanges();
            }
        }
    }
}
