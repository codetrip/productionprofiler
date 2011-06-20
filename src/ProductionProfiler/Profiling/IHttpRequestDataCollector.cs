﻿using System.Collections.Generic;
using System.Web;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling
{
    public interface IHttpRequestDataCollector
    {
        List<DataCollection> Collect(HttpRequest request);
    }
}