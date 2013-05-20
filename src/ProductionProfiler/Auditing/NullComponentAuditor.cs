using System;
namespace ProductionProfiler.Core.Auditing
{
    public class NullComponentAuditor : IComponentAuditor
    {
		public string Description { get; set; }
		public bool IsDebugEnabled { get; private set; }

	    public void Trace(Type component, string message, params object[] args)
	    {}

	    public void Info(Type component, string message, params object[] args)
	    {}

	    public void Warning(Type component, string message, params object[] args)
	    {}

	    public void Error(Type component, string message, params object[] args)
	    {}

	    public void Error(Type component, int eventId, string message, params object[] args)
	    {}

	    public void Error(Type component, Exception e)
	    {}

	    public void Error(Type component, int eventId, Exception e)
	    {}

    }
}