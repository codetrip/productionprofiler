using System;
using System.Collections.Generic;
using System.Text;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Persistence.Entities;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.RequestTiming;
using ProductionProfiler.Core.Serialization;
using ProductionProfiler.Core.Extensions;
using PetaPoco = ProductionProfiler.Core.Persistence;
using System.Linq;

namespace ProductionProfiler.Persistence.Sql
{
    public class SqlProfilerRepository : IProfilerRepository
    {
        private readonly SqlConfiguration _configuration;

        public SqlProfilerRepository(SqlConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Core.Persistence.Entities.Page<UrlToProfile> GetUrlsToProfile(PagingInfo pagingInfo)
        {
            using(var database = new Database(_configuration.ConnectionStringName))
            {
                var page = database.Page<UrlToProfile>(pagingInfo.PageNumber, pagingInfo.PageSize, "SELECT * FROM {0}.UrlToProfile".FormatWith(_configuration.SchemaName));
                return new Core.Persistence.Entities.Page<UrlToProfile>(page.Items, new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, (int)page.TotalItems));
            }
        }

        public UrlToProfile GetUrlToProfile(string url)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                return database.Single<UrlToProfile>("SELECT * FROM {0}.UrlToProfile WHERE Url = @0".FormatWith(_configuration.SchemaName), url);
            }
        }

        public List<UrlToProfile> GetCurrentUrlsToProfile()
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                return database.Fetch<UrlToProfile>("SELECT * FROM {0}.UrlToProfile WHERE Enabled = 1 AND (ProfilingCount > 0 OR ProfilingCount IS NULL)".FormatWith(_configuration.SchemaName));
            }
        }

        public void SaveUrlToProfile(UrlToProfile urlToProfile)
        {
            if (_configuration.GenerateIds && urlToProfile.Id != default(Guid))
            {
                urlToProfile.Id = Guid.NewGuid();
            }

            using (var database = new Database(_configuration.ConnectionStringName))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("IF NOT EXISTS(SELECT '*' FROM {0}.UrlToProfile WHERE Url = @0)".FormatWith(_configuration.SchemaName));
                sb.AppendLine("BEGIN");
                sb.AppendLine(database.InsertSql("{0}.UrlToProfile".FormatWith(_configuration.SchemaName), "Id", urlToProfile));
                sb.AppendLine("END");
                sb.AppendLine("ELSE");
                sb.AppendLine("BEGIN");
                sb.AppendLine(database.UpdateSql("{0}.UrlToProfile".FormatWith(_configuration.SchemaName), "Id", urlToProfile, urlToProfile.Id));
                sb.AppendLine("END");
                database.Execute(new PetaPoco.Sql(sb.ToString(), urlToProfile.Url, urlToProfile.ProfilingCount, urlToProfile.Server, urlToProfile.Enabled, urlToProfile.Id));
            }
        }

        public void DeleteUrlToProfile(string url)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Delete("{0}.UrlToProfile".FormatWith(_configuration.SchemaName), "Url", null, url);
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
                var data = database.Single<ProfiledRequestDataWrapper>("SELECT * FROM {0}.ProfiledRequestData WHERE Id = @0".FormatWith(_configuration.SchemaName), id);

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
                var page = database.Page<ProfiledRequestData>(pagingInfo.PageNumber, pagingInfo.PageSize, "SELECT DISTINCT Url FROM {0}.ProfiledRequestData".FormatWith(_configuration.SchemaName));
                return new Core.Persistence.Entities.Page<string>(page.Items.Select(i => i.Url), new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, (int)page.TotalItems));
            }
        }

        public void DeleteProfiledRequestDataByUrl(string url)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Delete("{0}.ProfiledRequestData".FormatWith(_configuration.SchemaName), "Url", null, url);
            }
        }

        public void DeleteProfiledRequestDataById(Guid id)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Delete("{0}.ProfiledRequestData".FormatWith(_configuration.SchemaName), "Id", null, id);
            }
        }

        public void SaveProfiledRequestData(ProfiledRequestData profiledRequestData)
        {
            if (_configuration.GenerateIds && profiledRequestData.Id != default(Guid))
            {
                profiledRequestData.Id = Guid.NewGuid();
            }

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

                database.Insert("{0}.ProfiledRequestData".FormatWith(_configuration.SchemaName), "Id", false, dataWrapper);
            }
        }

        public void SaveResponse(ProfiledResponse response)
        {
            if (_configuration.GenerateIds && response.Id != default(Guid))
            {
                response.Id = Guid.NewGuid();
            }
           
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Insert("{0}.ProfiledResponse".FormatWith(_configuration.SchemaName), "Id", false, response);
            }
        }

        public void SaveTimedRequest(TimedRequest timedRequest)
        {
            if (_configuration.GenerateIds && timedRequest.Id != default(Guid))
            {
                timedRequest.Id = Guid.NewGuid();
            }

            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Insert("{0}.TimedRequest".FormatWith(_configuration.SchemaName), "Id", false, timedRequest);
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
                return database.Single<ProfiledResponse>("SELECT * FROM {0}.ProfiledResponse WHERE Id = @0".FormatWith(_configuration.SchemaName), id);
            }
        }

        public void DeleteResponseById(Guid id)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Delete("{0}.ProfiledResponse".FormatWith(_configuration.SchemaName), "Id", null, id);
            }
        }

        public void DeleteResponseByUrl(string url)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Delete("{0}.ProfiledResponse".FormatWith(_configuration.SchemaName), "Url", null, url);
            }
        }

    }
}
