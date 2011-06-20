namespace ProductionProfiler.Core.Profiling.Entities
{
    public class ProfilerError
    {
        public long ErrorAtMilliseconds { get; set; }
        public string Message { get; set; }
        public ProfilerErrorType Type { private get; set; }
        public string TypeAsString { get { return Type.ToString(); } }
    }

    public enum ProfilerErrorType
    {
        Configuration,
        Runtime,
    }
}