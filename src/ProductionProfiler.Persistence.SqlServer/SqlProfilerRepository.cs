using System;
using System.Collections.Generic;
using System.Text;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Persistence.Entities;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Serialization;
using ProductionProfiler.Persistence.SqlServer.Entities;
using ProductionProfiler.Persistence.SqlServer.PetaPoco;
using ProductionProfiler.Core.Extensions;
using System.Linq;

namespace ProductionProfiler.Persistence.SqlServer
{
    public class SqlProfilerRepository : IProfilerRepository
    {
        private readonly SqlConfiguration _configuration;

        public SqlProfilerRepository(SqlConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Core.Persistence.Entities.Page<ProfiledRequest> GetProfiledRequests(PagingInfo pagingInfo)
        {
            using(var database = new Database(_configuration.ConnectionStringName))
            {
                var page = database.Page<ProfiledRequest>(pagingInfo.PageNumber, pagingInfo.PageSize, "SELECT * FROM {0}.ProfiledRequest".FormatWith(_configuration.SchemaName));
                return new Core.Persistence.Entities.Page<ProfiledRequest>(page.Items, new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, (int)page.TotalItems));
            }
        }

        public ProfiledRequest GetProfiledRequestByUrl(string url)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                return database.Single<ProfiledRequest>("SELECT * FROM {0}.ProfiledRequest WHERE Url = @0".FormatWith(_configuration.SchemaName), url);
            }
        }

        public List<ProfiledRequest> GetCurrentRequestsToProfile()
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                return database.Fetch<ProfiledRequest>("SELECT * FROM {0}.ProfiledRequest WHERE Enabled = 1 AND (ProfilingCount > 0 OR ProfilingCount IS NULL)".FormatWith(_configuration.SchemaName));
            }
        }

        public void SaveProfiledRequestWhenNotFound(ProfiledRequest profiledRequest)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("IF NOT EXISTS(SELECT '*' FROM {0}.ProfiledRequest WHERE Url = @0)".FormatWith(_configuration.SchemaName));
                sb.AppendLine("BEGIN");
                sb.AppendLine(database.InsertSql("{0}.ProfiledRequest".FormatWith(_configuration.SchemaName), "Id", profiledRequest));
                sb.AppendLine("END");
                database.Execute(new Sql(sb.ToString(), profiledRequest.Url, profiledRequest.ElapsedMilliseconds, profiledRequest.ProfilingCount, profiledRequest.ProfiledOnUtc, profiledRequest.Server, profiledRequest.HttpMethod, profiledRequest.Enabled));
            }
        }

        public void SaveProfiledRequest(ProfiledRequest profiledRequest)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("IF NOT EXISTS(SELECT '*' FROM {0}.ProfiledRequest WHERE Url = @0)".FormatWith(_configuration.SchemaName));
                sb.AppendLine("BEGIN");
                sb.AppendLine(database.InsertSql("{0}.ProfiledRequest".FormatWith(_configuration.SchemaName), "Id", profiledRequest));
                sb.AppendLine("END");
                sb.AppendLine("ELSE");
                sb.AppendLine("BEGIN");
                sb.AppendLine(database.UpdateSql("{0}.ProfiledRequest".FormatWith(_configuration.SchemaName), "Id", profiledRequest, profiledRequest.Id));
                sb.AppendLine("END");
                database.Execute(new Sql(sb.ToString(), profiledRequest.Url, profiledRequest.ElapsedMilliseconds, profiledRequest.ProfilingCount, profiledRequest.ProfiledOnUtc, profiledRequest.Server, profiledRequest.HttpMethod, profiledRequest.Enabled, profiledRequest.Id));
            }
        }

        public void DeleteProfiledRequest(string url)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Delete("{0}.ProfiledRequest".FormatWith(_configuration.SchemaName), "Url", null, url);
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestPreview> GetProfiledRequestDataPreviewByUrl(string url, PagingInfo pagingInfo)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                var results = database.Page<ProfiledRequestDataWrapper>(
                    pagingInfo.PageNumber, 
                    pagingInfo.PageSize,
                    "SELECT * FROM {0}.ProfiledRequestData WHERE Url = @0 ORDER BY CapturedOnUtc DESC".FormatWith(_configuration.SchemaName), url);

                if (results != null)
                {
                    return new Core.Persistence.Entities.Page<ProfiledRequestPreview>(
                        results.Items.Select(p =>
                        {
                            var data = BinarySerializer<ProfiledRequestData>.Deserialize(p.Data);
                            return new ProfiledRequestPreview
                            {
                                CapturedOnUtc = data.CapturedOnUtc,
                                ElapsedMilliseconds = data.ElapsedMilliseconds,
                                Server = data.Server,
                                Id = data.Id,
                                Url = data.Url
                            };
                        }), new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, (int)results.TotalItems));
                }

                return new Core.Persistence.Entities.Page<ProfiledRequestPreview>(new ProfiledRequestPreview[0], new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, 0));
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

        public Core.Persistence.Entities.Page<string> GetDistinctProfiledRequestUrls(PagingInfo pagingInfo)
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
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                var dataWrapper = new ProfiledRequestDataWrapper
                {
                    Id = profiledRequestData.Id,
                    Url = profiledRequestData.Url,
                    Data = BinarySerializer<ProfiledRequestData>.Serialize(profiledRequestData),
                    CapturedOnUtc = profiledRequestData.CapturedOnUtc
                };

                database.Insert("{0}.ProfiledRequestData".FormatWith(_configuration.SchemaName), "Id", false, dataWrapper);
            }
        }

        public void SaveResponse(ProfiledResponse response)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Insert("{0}.ProfiledResponse".FormatWith(_configuration.SchemaName), "Id", false, response);
            }
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
