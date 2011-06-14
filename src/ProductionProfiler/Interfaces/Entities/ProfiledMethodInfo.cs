using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProductionProfiler.Interfaces.Entities
{
    [Serializable]
    public class ProfiledMethodInfo
    {
        [NonSerialized]
        private ProfiledMethodInfo _parentMethod;
        [NonSerialized]
        private Stopwatch _watch;

        public List<LogMessage> LogMessages { get; set; }
        public List<ProfiledMethodInfo> InnerMethods { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public long StartedAtMilliseconds { get; set; }
        public long StoppedAtMilliseconds { get; set; }
        public string MethodName { get; set; }
        public bool ErrorInMethod { get; set; }

        public ProfiledMethodInfo GetParentMethod()
        {
            return _parentMethod;
        }

        public void SetParentMethod(ProfiledMethodInfo parentMethod)
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

        public ProfiledMethodInfo()
        {
            InnerMethods = new List<ProfiledMethodInfo>();
            LogMessages = new List<LogMessage>();
        }
    }
}