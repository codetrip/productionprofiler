using System.Diagnostics;
using System.Web;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.RequestTiming.Entities;

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
        private readonly IProfilerRepository _repository;
        private readonly ProfilerConfiguration _configuration;

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
}