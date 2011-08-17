
using System;
using ProductionProfiler.Core.Auditing;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core
{
    public abstract class ComponentBase
    {
        public IComponentAuditor Auditor { protected get; set; }

        public void Trace(string message, params object[] args)
        {
            Auditor.Trace(GetType(), message, args);
        }

        public void Info(string message, params object[] args)
        {
            Auditor.Info(GetType(), message, args);
        }

        public void Warning(string message, params object[] args)
        {
            Auditor.Warning(GetType(), message, args);
        }

        public void Error(Exception e)
        {
            Auditor.Error(GetType(), e);

            if (ProfilerContext.Configuration.ReportException != null)
            {
                ProfilerContext.Configuration.ReportException(e);
            }
        }
    }
}