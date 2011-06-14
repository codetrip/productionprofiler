
using System;
using System.IO;
using System.IO.Compression;
using System.Web;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;

namespace ProductionProfiler.Handlers
{
    public class ResourceRequestHandler : IRequestHandler
    {
        private static readonly DateTime _assemblyLastModified = new FileInfo(typeof (ResourceRequestHandler).Assembly.Location).LastWriteTime;

        public void HandleRequest(HttpContext context, RequestInfo requestInfo)
        {
            HttpResponse response = context.Response;
            using (Stream stream = typeof(ResourceRequestHandler).Assembly.GetManifestResourceStream("ProductionProfiler.Resources." + requestInfo.ResourceName))
            {
                if (stream != null)
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }

            if (context.Request.Headers.Get(Constants.HttpHeaders.AcceptEncoding) != null && context.Request.Headers.Get(Constants.HttpHeaders.AcceptEncoding).Contains(Constants.RequestEncoding.GZip))
            {
                context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                context.Response.AppendHeader(Constants.HttpHeaders.ContentEncoding, Constants.RequestEncoding.GZip);
                context.Response.Cache.VaryByHeaders[Constants.HttpHeaders.AcceptEncoding] = true;
            }

            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetLastModified(_assemblyLastModified);
            context.Response.Cache.VaryByHeaders[Constants.HttpHeaders.IfModifiedSince] = true;
            context.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(1));

            context.Response.AppendHeader(Constants.HttpHeaders.ContentType, requestInfo.ContentType);
        }
    }
}
