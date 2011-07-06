using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Persistence.Entities;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Serialization;
using PetaPoco = ProductionProfiler.Core.Persistence;

namespace ProductionProfiler.Persistence.SQLite
{
    public class SQLiteProfilerRepository : IProfilerRepository
    {
        private readonly SQLiteConfiguration _configuration;

        public SQLiteProfilerRepository(SQLiteConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Core.Persistence.Entities.Page<ProfiledRequest> GetProfiledRequests(PagingInfo pagingInfo)
        {
            using(var database = new Database(_configuration.ConnectionStringName))
            {
                var page = database.Page<ProfiledRequest>(pagingInfo.PageNumber, pagingInfo.PageSize, "SELECT * FROM ProfiledRequest");
                return new Core.Persistence.Entities.Page<ProfiledRequest>(page.Items, new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, (int)page.TotalItems));
            }
        }

        public ProfiledRequest GetProfiledRequestByUrl(string url)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                return database.Single<ProfiledRequest>("SELECT * FROM ProfiledRequest WHERE Url = @0", url);
            }
        }

        public List<ProfiledRequest> GetCurrentRequestsToProfile()
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                return database.Fetch<ProfiledRequest>("SELECT * FROM ProfiledRequest WHERE Enabled = 1 AND (ProfilingCount > 0 OR ProfilingCount IS NULL)");
            }
        }

        public void SaveProfiledRequestWhenNotFound(ProfiledRequest profiledRequest)
        {
            profiledRequest.Id = Guid.NewGuid();

            using (var database = new Database(_configuration.ConnectionStringName))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("IF NOT EXISTS(SELECT '*' FROM ProfiledRequest WHERE Url = @0)");
                sb.AppendLine("BEGIN");
                sb.AppendLine(database.InsertSql("ProfiledRequest", "Id", profiledRequest));
                sb.AppendLine("END");
                database.Execute(new PetaPoco.Sql(sb.ToString(), profiledRequest.Url, profiledRequest.ElapsedMilliseconds, profiledRequest.ProfilingCount, profiledRequest.ProfiledOnUtc, profiledRequest.Server, profiledRequest.HttpMethod, profiledRequest.Enabled));
            }
        }

        public void SaveProfiledRequest(ProfiledRequest profiledRequest)
        {
            profiledRequest.Id = Guid.NewGuid();

            using (var database = new Database(_configuration.ConnectionStringName))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("IF NOT EXISTS(SELECT '*' FROM ProfiledRequest WHERE Url = @0)");
                sb.AppendLine("BEGIN");
                sb.AppendLine(database.InsertSql("ProfiledRequest", "Id", profiledRequest));
                sb.AppendLine("END");
                sb.AppendLine("ELSE");
                sb.AppendLine("BEGIN");
                sb.AppendLine(database.UpdateSql("ProfiledRequest", "Id", profiledRequest, profiledRequest.Id));
                sb.AppendLine("END");
                database.Execute(new PetaPoco.Sql(sb.ToString(), profiledRequest.Url, profiledRequest.ElapsedMilliseconds, profiledRequest.ProfilingCount, profiledRequest.ProfiledOnUtc, profiledRequest.Server, profiledRequest.HttpMethod, profiledRequest.Enabled, profiledRequest.Id));
            }
        }

        public void DeleteProfiledRequest(string url)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Delete("ProfiledRequest", "Url", null, url);
            }
        }

        public Core.Persistence.Entities.Page<ProfiledRequestPreview> GetProfiledRequestDataPreviewByUrl(string url, PagingInfo pagingInfo)
        {
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                var results = database.Page<ProfiledRequestDataWrapper>(
                    pagingInfo.PageNumber, 
                    pagingInfo.PageSize,
                    "SELECT * FROM ProfiledRequestData WHERE Url = @0 ORDER BY CapturedOnUtc DESC", url);

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
                var data = database.Single<ProfiledRequestDataWrapper>("SELECT * FROM ProfiledRequestData WHERE Id = @0", id);

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
            profiledRequestData.Id = Guid.NewGuid();

            using (var database = new Database(_configuration.ConnectionStringName))
            {
                var dataWrapper = new ProfiledRequestDataWrapper
                {
                    Id = profiledRequestData.Id,
                    Url = profiledRequestData.Url,
                    Data = BinarySerializer<ProfiledRequestData>.Serialize(profiledRequestData),
                    CapturedOnUtc = profiledRequestData.CapturedOnUtc
                };

                database.Insert("ProfiledRequestData", "Id", false, dataWrapper);
            }
        }

        public void SaveResponse(ProfiledResponse response)
        {
            response.Id = Guid.NewGuid();
           
            using (var database = new Database(_configuration.ConnectionStringName))
            {
                database.Insert("ProfiledResponse", "Id", false, response);
            }
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
