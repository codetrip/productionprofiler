using System;
using System.Collections.Generic;
using System.Text;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Persistence.Entities;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Persistence.SqlServer.PetaPoco;
using ProductionProfiler.Core.Extensions;

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
                return database.Fetch<ProfiledRequest>("SELECT * FROM {0}.ProfiledRequest WHERE Enabled = 1 AND ProfilingCount > 0".FormatWith(_configuration.SchemaName));
            }
        }

        public void SaveProfiledRequestWhenNotFound(ProfiledRequest profiledRequest)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                StringBuilder sb = new StringBuilder("IF NOT EXISTS(SELECT '*' FROM {0}.ProfiledRequest WHERE Url = @0".FormatWith(_configuration.SchemaName));
                sb.AppendLine("BEGIN");
                sb.AppendLine(database.InsertSql("{0}.ProfiledRequest".FormatWith(_configuration.SchemaName), "Id", profiledRequest));
                sb.AppendLine("END");
                database.Execute(new Sql(sb.ToString(), profiledRequest.Url));
            }
        }

        public void SaveProfiledRequest(ProfiledRequest profiledRequest)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                StringBuilder sb = new StringBuilder("IF NOT EXISTS(SELECT '*' FROM {0}.ProfiledRequest WHERE Url = @0".FormatWith(_configuration.SchemaName));
                sb.AppendLine("BEGIN");
                sb.AppendLine(database.InsertSql("{0}.ProfiledRequest".FormatWith(_configuration.SchemaName), "Id", profiledRequest));
                sb.AppendLine("END");
                sb.AppendLine("ELSE");
                sb.AppendLine("BEGIN");
                sb.AppendLine(database.UpdateSql("{0}.ProfiledRequest".FormatWith(_configuration.SchemaName), "Id", profiledRequest, profiledRequest.Id));
                sb.AppendLine("END");
                database.Execute(new Sql(sb.ToString(), profiledRequest.Url));
            }
        }

        public void DeleteProfiledRequest(string url)
        {
            throw new NotImplementedException();
        }

        public Core.Persistence.Entities.Page<ProfiledRequestPreview> GetProfiledRequestDataPreviewByUrl(string url, PagingInfo pagingInfo)
        {
            throw new NotImplementedException();
        }

        public ProfiledRequestData GetProfiledRequestDataById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Core.Persistence.Entities.Page<string> GetDistinctProfiledRequestUrls(PagingInfo pagingInfo)
        {
            throw new NotImplementedException();
        }

        public void DeleteProfiledRequestDataByUrl(string url)
        {
            throw new NotImplementedException();
        }

        public void DeleteProfiledRequestDataById(Guid id)
        {
            throw new NotImplementedException();
        }

        public void SaveProfiledRequestData(ProfiledRequestData profiledRequestData)
        {
            throw new NotImplementedException();
        }

        public void SaveResponse(ProfiledResponse response)
        {
            throw new NotImplementedException();
        }

        public ProfiledResponse GetResponseById(Guid id)
        {
            throw new NotImplementedException();
        }

        public void DeleteResponseById(Guid id)
        {
            throw new NotImplementedException();
        }

        public void DeleteResponseByUrl(string url)
        {
            throw new NotImplementedException();
        }
    }
}
