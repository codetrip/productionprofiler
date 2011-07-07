using System;
using System.Web;

namespace ProductionProfiler.Core.Factory
{
    /// <summary>
    ///  Class added for the purpose of Testing (Unit) the Web App with HttpContext
    ///  Dependency Injection used to pass the custom context object with the session dic object
    ///  Factory used to decided which object to pass real HttpContext or the mock HttpContext
    /// </summary>
    public static class HttpContextFactory
    {
        [ThreadStatic]
        private static HttpContext _mockHttpContext;

        public static void SetHttpContext(HttpContext httpContext)
        {
            _mockHttpContext = httpContext;
        }

        public static void ResetHttpContext()
        {
            _mockHttpContext = null;
        }

        public static HttpContext GetHttpContext()
        {
            if (_mockHttpContext != null)
            {
                return _mockHttpContext;
            }

            if (HttpContext.Current != null)
            {
                return HttpContext.Current;
            }
            return null;
        }
    }
}
