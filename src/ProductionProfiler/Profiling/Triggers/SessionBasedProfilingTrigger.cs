using System;
using System.Web;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Web;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Profiling.Triggers
{
    public sealed class SessionBasedProfilingTrigger : ComponentBase, IProfilingTrigger
    {
        private const string SessionCookieName = "profiler-session";
        private const string SessionCookieId = "id";
        private const string SessionCookieUserName = "user";
        private const string SessionQueryParameter = "psid";

        private readonly ICookieManager _cookieManager;
        private readonly ProfilerConfiguration _configuration;

        public SessionBasedProfilingTrigger(ICookieManager cookieManager, ProfilerConfiguration configuration)
        {
            _cookieManager = cookieManager;
            _configuration = configuration;
        }

        public bool TriggerProfiling(HttpContext context)
        {
            if (!Enabled)
            {
                Trace("Session based trigger is disabled, returning false");
                return false;
            }

            if(_cookieManager.Get(SessionCookieName) != string.Empty)
            {
                return true;
            }

            string sessionUserId = context.Request.QueryString.Get(SessionQueryParameter);

            if (sessionUserId.IsNotNullOrEmpty() && _configuration.AuthorisedForSession(sessionUserId))
            {
                _cookieManager.Set(SessionCookieName, SessionCookieId, Guid.NewGuid().ToString(), null);
                _cookieManager.Set(SessionCookieName, SessionCookieUserName, sessionUserId, null);
                return true;
            }

            return false;
        }

        public void AugmentProfiledRequestData(ProfiledRequestData data)
        {
            try
            {
                Guid sessionId = new Guid(_cookieManager.Get(SessionCookieName, SessionCookieId));
                data.SessionId = sessionId;
                data.SessionUserId = _cookieManager.Get(SessionCookieName, SessionCookieUserName);
            }
            catch (Exception)
            {}
        }

        private bool Enabled
        {
            get
            {
                return _configuration.Settings.ContainsKey(ProfilerConfiguration.SettingKeys.SessionTriggerEnabled) &&
                       _configuration.Settings[ProfilerConfiguration.SettingKeys.SessionTriggerEnabled] == "true";
            }
        }
    }
}
