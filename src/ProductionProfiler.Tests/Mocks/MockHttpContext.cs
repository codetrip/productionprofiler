using System.IO;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace ProductionProfiler.Tests.Mocks
{
    public class MockHttpContext
    {
        private readonly HttpContext _context;

        private MockHttpContext()
        { }

        public MockHttpContext(bool isSecure) : 
            this(string.Empty, isSecure) { }

        public MockHttpContext(string query, bool isSecure)
            : this()
        {

            Thread.GetDomain().SetData(".appPath", @"C:\Inetpub\wwwroot\webapp");
            Thread.GetDomain().SetData(".appVPath", "/webapp");
            Thread.GetDomain().SetData(".hostingInstallDir", HttpRuntime.AspInstallDirectory);
            SimpleWorkerRequest request = new WorkerRequest("default.aspx", query, new StringWriter(), isSecure);
            _context = new HttpContext(request);
        }

        public HttpContext Context
        {
            get
            {
                return this._context;
            }
        }
    }

    public class WorkerRequest : SimpleWorkerRequest
    {
        private readonly bool _isSecure;

        public WorkerRequest(string page, string query, TextWriter output, bool isSecure)
            : base(page, query, output)
        {
            _isSecure = isSecure;
        }

        public override bool IsSecure()
        {
            return _isSecure;
        }
    }
}
