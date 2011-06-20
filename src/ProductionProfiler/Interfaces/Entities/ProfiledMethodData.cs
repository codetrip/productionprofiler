using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProductionProfiler.Interfaces.Entities
{
    [Serializable]
    public class ProfiledMethodData
    {
        [NonSerialized]
        private ProfiledMethodData _parentMethod;
        [NonSerialized]
        private Stopwatch _watch;

        public List<ProfilerMessage> Messages { get; set; }
        public List<ProfiledMethodData> Methods { get; set; }
        public List<ThrownException> Exceptions { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public long StartedAtMilliseconds { get; set; }
        public long StoppedAtMilliseconds { get; set; }
        public string MethodName { get; set; }
        public bool ErrorInMethod { get; set; }

        public ProfiledMethodData GetParentMethod()
        {
            return _parentMethod;
        }

        public void SetParentMethod(ProfiledMethodData parentMethod)
        {
            _parentMethod = parentMethod;
        }

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

        public ProfiledMethodData()
        {
            Methods = new List<ProfiledMethodData>();
            Messages = new List<ProfilerMessage>();
            Exceptions = new List<ThrownException>();
        }
    }
}