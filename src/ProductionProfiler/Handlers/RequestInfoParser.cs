
using System;
using System.Web;
using ProductionProfiler.Core.Interfaces.Entities;
using ProductionProfiler.Core.Interfaces.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public class RequestInfoParser
    {
        public static RequestInfo Parse(HttpRequest request)
        {
            RequestInfo requestInfo = new RequestInfo();

            string pageSize = request.QueryString.Get(Constants.Querystring.PageSize);
            string pageNumber = request.QueryString.Get(Constants.Querystring.PageNumber);

            requestInfo.Paging = new PagingInfo(
                string.IsNullOrEmpty(pageNumber) ? 1 : int.Parse(pageNumber),
                string.IsNullOrEmpty(pageSize) ? 15 : int.Parse(pageSize));

            requestInfo.Handler = request.QueryString.Get(Constants.Querystring.Handler) ?? string.Empty;
            requestInfo.Action = request.QueryString.Get(Constants.Querystring.Action) ?? string.Empty;
            requestInfo.ContentType = request.QueryString.Get(Constants.Querystring.ContentType) ?? string.Empty;
            requestInfo.ResourceName = request.QueryString.Get(Constants.Querystring.Resource) ?? string.Empty;
            requestInfo.Url = request.QueryString.Get(Constants.Querystring.Url) ?? string.Empty;
            requestInfo.Id = request.QueryString.Get(Constants.Querystring.Id) == null ? Guid.Empty : Guid.Parse(request.QueryString.Get(Constants.Querystring.Id));
            requestInfo.Form = request.Form;

            return requestInfo;
        }
    }
}
