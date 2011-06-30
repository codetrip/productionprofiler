using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProductionProfiler.Core.Profiling.Entities
{
    [Serializable]
    public class MethodData
    {
        [NonSerialized]
        private Stopwatch _watch;

        public List<ProfilerMessage> Messages { get; set; }
        public List<MethodData> Methods { get; set; }
        public List<ThrownException> Exceptions { get; set; }
        public List<DataCollection> Data { get; set; }
        public List<DataCollectionItem> Arguments { get; set; }
        public DataCollectionItem ReturnValue { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public long StartedAtMilliseconds { get; set; }
        public long StoppedAtMilliseconds { get; set; }
        public string MethodName { get; set; }

        public void Start()
        {
            _watch = Stopwatch.StartNew();
        }

        public long Stop()
        {
            _watch.Stop();
            return _watch.ElapsedMilliseconds;
        }

        public long Elapsed()
        {
            return _watch.ElapsedMilliseconds;
        }

        public MethodData()
        {
            Methods = new List<MethodData>();
            Messages = new List<ProfilerMessage>();
            Exceptions = new List<ThrownException>();
            Data = new List<DataCollection>();
        }
    }
}