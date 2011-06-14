
using System;
using ProductionProfiler.Interfaces.Entities;

namespace ProductionProfiler.Interfaces
{
    public interface IProfiledRequestDataRepository : IRepository<ProfiledRequestData, Guid>
    {
        Page<ProfiledRequestDataPreview> GetPreviewByUrl(string url, PagingInfo pagingInfo);
        Page<string> GetDistinctUrls(PagingInfo pagingInfo);
        void DeleteByUrl(string url);
    }
}
