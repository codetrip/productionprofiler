using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Persistence.Entities;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.RequestTiming;
using ProductionProfiler.Core.Serialization;
using PetaPoco = ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Persistence.SQLite
{
    public class SQLiteProfilerRepository : IProfilerRepository
    {
        private readonly SQLiteConfiguration _configuration;

        public SQLiteProfilerRepository(SQLiteConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Core.Persistence.Entities.Page<UrlToProfile> GetUrlsToProfile(PagingInfo pagingInfo)
        {
            using(var database = new Database(_configuration.ConnectionStringName))
            {
                var page = database.Page<UrlToProfile>(pagingInfo.PageNumber, pagingInfo.PageSize, "SELECT * FROM UrlToProfile");
                return new Core.Persistence.Entities.Page<UrlToProfile>(page.Items, new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, (int)page.TotalItems));
            }
        }

        public UrlToProfile GetUrlToProfile(string url)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                return database.Single<UrlToProfile>("SELECT * FROM UrlToProfile WHERE Url = @0", url);
            }
        }

        public List<UrlToProfile> GetCurrentUrlsToProfile()
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                return database.Fetch<UrlToProfile>("SELECT * FROM UrlToProfile WHERE Enabled = 1 AND (ProfilingCount > 0 OR ProfilingCount IS NULL)");
            }
        }

        public void SaveUrlToProfile(UrlToProfile urlToProfile)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(database.InsertSql("UrlToProfile", "Id", urlToProfile, false).Replace("INSERT INTO", "REPLACE INTO"));
                database.Execute(new Sql(sb.ToString(), urlToProfile.Id, urlToProfile.Url, urlToProfile.ProfilingCount, urlToProfile.Server, urlToProfile.Enabled, urlToProfile.Id));
            }
        }

        public void DeleteUrlToProfile(string url)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Delete("UrlToProfile", "Url", null, url);
            }
        }

        private Core.Persistence.Entities.Page<ProfiledRequestDataPreview> DoGetPreview(PetaPoco.Page<ProfiledRequestDataWrapper> results, PagingInfo pagingInfo)
        {
            if (results != null)
            {
                return new Core.Persistence.Entities.Page<ProfiledRequestDataPreview>(
                    results.Items.Select(p =>
                    {
                        var data = BinarySerializer<ProfiledRequestData>.Deserialize(p.Data);
                        return new ProfiledRequestDataPreview
                        {
                            CapturedOnUtc = data.CapturedOnUtc,
                            ElapsedMilliseconds = data.ElapsedMilliseconds,
                            Server = data.Server,
                            Id = data.Id,
                            Url = data.Url
                        };
                    }), new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, (int)results.TotalItems));
            }

            return new Core.Persistence.Entities.Page<ProfiledRequestDataPreview>(new ProfiledRequestDataPreview[0], new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, 0));
        }

        public Core.Persistence.Entities.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewByUrl(string url, PagingInfo pagingInfo)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                var results = database.Page<ProfiledRequestDataWrapper>(
                    pagingInfo.PageNumber, 
                    pagingInfo.PageSize,
                    "SELECT * FROM ProfiledRequestData WHERE Url = @0 ORDER BY CapturedOnUtc DESC", url);

                return DoGetPreview(results, pagingInfo);
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewBySessionId(Guid sessionId, PagingInfo pagingInfo)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                var results = database.Page<ProfiledRequestDataWrapper>(
                    pagingInfo.PageNumber,
                    pagingInfo.PageSize,
                    "SELECT * FROM ProfiledRequestData WHERE SessionId = @0 ORDER BY CapturedOnUtc DESC", sessionId);

                return DoGetPreview(results, pagingInfo);
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewBySessionUserId(string sessionUserId, PagingInfo pagingInfo)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                var results = database.Page<ProfiledRequestDataWrapper>(
                    pagingInfo.PageNumber,
                    pagingInfo.PageSize,
                    "SELECT * FROM ProfiledRequestData WHERE SessionUserId = @0 ORDER BY CapturedOnUtc DESC", sessionUserId);

                return DoGetPreview(results, pagingInfo);
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestDataPreview> GetProfiledRequestDataPreviewBySamplingId(Guid samplingId, PagingInfo pagingInfo)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                var results = database.Page<ProfiledRequestDataWrapper>(
                    pagingInfo.PageNumber,
                    pagingInfo.PageSize,
                    "SELECT * FROM ProfiledRequestData WHERE SamplingId = @0 ORDER BY CapturedOnUtc DESC", samplingId);

                return DoGetPreview(results, pagingInfo);
            }
        }

        public ProfiledRequestData GetProfiledRequestDataById(Guid id)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                var data = database.Single<ProfiledRequestDataWrapper>("SELECT * FROM ProfiledRequestData WHERE Id = @0", id);

                if(data != null)
                {
                    return BinarySerializer<ProfiledRequestData>.Deserialize(data.Data);
                }
            }

            return null;
        }

        public Core.Persistence.Entities.Page<string> GetDistinctUrlsToProfile(PagingInfo pagingInfo)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                var page = database.Page<ProfiledRequestData>(pagingInfo.PageNumber, pagingInfo.PageSize, "SELECT DISTINCT Url FROM ProfiledRequestData");
                return new Core.Persistence.Entities.Page<string>(page.Items.Select(i => i.Url), new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, (int)page.TotalItems));
            }
        }

        public void DeleteProfiledRequestDataByUrl(string url)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Delete("ProfiledRequestData", "Url", null, url);
            }
        }

        public void DeleteProfiledRequestDataById(Guid id)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Delete("ProfiledRequestData", "Id", null, id);
            }
        }

        public void SaveProfiledRequestData(ProfiledRequestData profiledRequestData)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                var dataWrapper = new ProfiledRequestDataWrapper
                {
                    Id = profiledRequestData.Id,
                    Url = profiledRequestData.Url,
                    Data = BinarySerializer<ProfiledRequestData>.Serialize(profiledRequestData),
                    CapturedOnUtc = profiledRequestData.CapturedOnUtc,
                    SamplingId = profiledRequestData.SamplingId,
                    SessionId = profiledRequestData.SessionId,
                    SessionUserId = profiledRequestData.SessionUserId
                };

                database.Insert("ProfiledRequestData", "Id", false, dataWrapper);
            }
        }

        public void SaveResponse(ProfiledResponse response)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Insert("ProfiledResponse", "Id", false, response);
            }
        }


        public void SaveTimedRequest(TimedRequest timedRequest)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Insert("TimedRequest", "Id", false, timedRequest);
            }
        }

        public Core.Persistence.Entities.Page<TimedRequest> GetLongRequests(PagingInfo paging)
        {
            throw new NotImplementedException();
        }

        public ProfiledResponse GetResponseById(Guid id)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                return database.Single<ProfiledResponse>("SELECT * FROM ProfiledResponse WHERE Id = @0", id);
            }
        }

        public void DeleteResponseById(Guid id)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Delete("ProfiledResponse", "Id", null, id);
            }
        }

        public void DeleteResponseByUrl(string url)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Delete("ProfiledResponse", "Url", null, url);
            }
        }
    }
}
