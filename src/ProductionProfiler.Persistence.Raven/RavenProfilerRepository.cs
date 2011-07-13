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
        public const string ProfiledRequestIndexName = "ProfiledRequestIndex";
        public const string ProfiledRequestDataIndexName = "ProfiledRequestDataIndex";
        public const string ProfiledResponseIndexName = "ProfiledResponseIndex";

        private readonly DocumentStore _database;

        public RavenProfilerRepository(DocumentStore database)
        {
            _database = database;
        }

        public Core.Persistence.Entities.Page<ProfiledRequest> GetProfiledRequests(PagingInfo pagingInfo)
        {
            using (var session = _database.OpenSession())
            {
                var documentQuery = session.Advanced.LuceneQuery<ProfiledRequest>(ProfiledRequestIndexName)
                    .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize)
                    .WaitForNonStaleResults(new TimeSpan(0, 0, 3));

                return new Core.Persistence.Entities.Page<ProfiledRequest>(
                    documentQuery.ToList(), 
                    new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, documentQuery.QueryResult.TotalResults));
            }
        }

        public ProfiledRequest GetProfiledRequestByUrl(string url)
        {
            using (var session = _database.OpenSession())
            {
                return session.Query<ProfiledRequest>(ProfiledRequestIndexName).Where(r => r.Url == url).FirstOrDefault();
            }
        }

        public List<ProfiledRequest> GetCurrentRequestsToProfile()
        {
            using (var session = _database.OpenSession())
            {
                return session.Query<ProfiledRequest>(ProfiledRequestIndexName)
                    .Where(req => req.Enabled)
                    .Where(req => req.ProfilingCount == null || req.ProfilingCount > 0)
                    .ToList();
            }
        }

        public void SaveProfiledRequestWhenNotFound(ProfiledRequest profiledRequest)
        {
            using (var session = _database.OpenSession())
            {
                var request = session.Query<ProfiledRequest>(ProfiledRequestIndexName)
                    .Where(r => r.Url == profiledRequest.Url)
                    .Select(r => r.Url)
                    .FirstOrDefault();

                if (request == null)
                {
                    session.Store(profiledRequest);
                    session.SaveChanges();
                }
            }
        }

        public void SaveProfiledRequest(ProfiledRequest profiledRequest)
        {
            using (var session = _database.OpenSession())
            {
                // HRB: removed this code as it's causing problems ("Error: Attempted to associated a different object with id"),
                // and it's not necessary - Raven will simply overwrite the stored profile, which is what we want.
                
                /*
                var request = session.Query<ProfiledRequest>(ProfiledRequestIndexName)
                    .Where(r => r.Url == profiledRequest.Url)
                    .FirstOrDefault();

                //if one exists delete it and replace with the new incoming request
                if (request != null)
                {
                    session.Delete(request);
                }
                */

                session.Store(profiledRequest);
                session.SaveChanges();
            }
        }

        public void DeleteProfiledRequest(string url)
        {
            using (var session = _database.OpenSession())
            {
                var item = session.Query<ProfiledRequest>(ProfiledRequestIndexName).Where(r => r.Url == url).FirstOrDefault();

                if (item != null)
                {
                    session.Delete(item);
                    session.SaveChanges();
                }
            }
        }

        public Core.Persistence.Entities.Page<string> GetDistinctProfiledRequestUrls(PagingInfo pagingInfo)
        {
            using (var session = _database.OpenSession())
            {
                var documentQuery = session.Query<ProfiledRequestData>(ProfiledRequestDataIndexName)
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
                return session.Query<ProfiledRequestData>(ProfiledRequestDataIndexName).Where(r => r.Id == id).FirstOrDefault();
            }
        }

        public void DeleteProfiledRequestDataById(Guid id)
        {
            using (var session = _database.OpenSession())
            {
                var item = session.Query<ProfiledRequestData>(ProfiledRequestDataIndexName).Where(r => r.Id == id).FirstOrDefault();

                if (item != null)
                {
                    session.Delete(item);
                    session.SaveChanges();
                }
            }
        }

        public void SaveProfiledRequestData(ProfiledRequestData entity)
        {
            using (var session = _database.OpenSession())
            {
                session.Store(entity);
                session.SaveChanges();
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestPreview> GetProfiledRequestDataPreviewByUrl(string url, PagingInfo pagingInfo)
        {
            using (var session = _database.OpenSession())
            {
                var linqQuery = session.Query<ProfiledRequestData>(ProfiledRequestDataIndexName).Where(r => r.Url == url);
                var pagination = new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, linqQuery.Count());
                var results = linqQuery.OrderByDescending(r => r.CapturedOnUtc)
                    .Select(r => new ProfiledRequestPreview
                    {
                        CapturedOnUtc = r.CapturedOnUtc,
                        ElapsedMilliseconds = r.ElapsedMilliseconds,
                        Id = r.Id,
                        Server = r.Server,
                        Url = r.Url
                    }).Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize).Take(pagingInfo.PageSize);

                return new Core.Persistence.Entities.Page<ProfiledRequestPreview>(results.ToList(), pagination);
            }
        }

        public void DeleteProfiledRequestDataByUrl(string url)
        {
            using (var session = _database.OpenSession())
            {
                foreach (var document in session.Query<ProfiledRequestData>(ProfiledRequestDataIndexName).Where(r => r.Url == url))
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
