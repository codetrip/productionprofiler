using System.Collections.Generic;
using ProductionProfiler.Core.Persistence.Entities;

namespace ProductionProfiler.Core.Handlers.Entities
{
    public class JsonResponse
    {
        public object Data { get; set; }
        public string Redirect { get; set; }
        public Pagination Paging { get; set; }
        public List<ModelValidationError> Errors { get; set; }
        public bool Success { get; set; }

        public JsonResponse()
        {
            Success = true;
        }
    }
}
