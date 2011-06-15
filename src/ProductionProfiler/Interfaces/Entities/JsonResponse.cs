
namespace ProductionProfiler.Interfaces.Entities
{
    public class JsonResponse
    {
        public object Data { get; set; }
        public string Redirect { get; set; }
        public Pagination Paging { get; set; }
    }
}
