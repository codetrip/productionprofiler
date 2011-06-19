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

        public List<LogMessage> LogMessages { get; set; }
        public List<ProfiledMethodData> InnerMethods { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public long StartedAtMilliseconds { get; set; }
        public long StoppedAtMilliseconds { get; set; }
        public string MethodName { get; set; }
        public bool ErrorInMethod { get; set; }
        public List<ThrownException> Exceptions { get; set; }

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
            InnerMethods = new List<ProfiledMethodData>();
            LogMessages = new List<LogMessage>();
            Exceptions = new List<ThrownException>();
        }
    }
}