using System;
using System.Diagnostics;
using System.Web;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.RequestTiming
{
    public interface IRequestTimer : IDoNotWantToBeProfiled
    {
        void Start(HttpContext context);
        void Stop(HttpContext context);
    }

    public class RequestTimer : IRequestTimer
    {
        private readonly Stopwatch _sw = new Stopwatch();
        private IProfilerRepository _repository;
        private ProfilerConfiguration _configuration;

        public RequestTimer(IProfilerRepository repository, ProfilerConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public void Start(HttpContext context)
        {
            _sw.Start();
        }

        public void Stop(HttpContext context)
        {
            _sw.Stop();

            if (_sw.ElapsedMilliseconds > _configuration.LongRequestThresholdMs)
                _repository.SaveTimedRequest(new TimedRequest(context.Request.Url.ToString(), _sw.ElapsedMilliseconds));
            
        }
    }

    [Serializable]
    public class TimedRequest : IAsyncPersistable
    {
        public TimedRequest(string url, long durationMs) :this()
        {
            Url = url;
            DurationMs = durationMs;
            RequestUtc = DateTime.UtcNow;
            Server = Environment.MachineName;
            UrlPathAndQuery = new Uri(url).PathAndQuery;
        }

        public TimedRequest()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string UrlPathAndQuery { get; set; }
        public long DurationMs { get; set; }
        public string Server { get; set; }

        public string FriendlyRequestLocal
        {
            get { return RequestUtc.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss"); }
        }

        public DateTime RequestUtc { get; set; }
    }
}