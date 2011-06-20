using System;

namespace ProductionProfiler.Core.Interfaces.Entities
{
    /// <summary>
    /// Paging request information.
    /// </summary>
    [Serializable]
    public class PagingInfo
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public PagingInfo()
        { }

        public PagingInfo(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
