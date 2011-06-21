using System.IO;
using System.Text;

namespace ProductionProfiler.Core.Profiling
{
    public class StoreResponseFilter : Stream, IResponseFilter
    {
        private readonly Stream _responseStream;
        private readonly StringBuilder _response;

        public StoreResponseFilter(Stream inputStream)
        {
            _responseStream = inputStream;
            _response = new StringBuilder();
        }

        public override bool CanRead
        {
            get { return true;}
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Close()
        {
            _responseStream.Close ();
        }

        public override void Flush()
        {
            _responseStream.Flush ();
        }

        public override long Length
        {
            get { return 0; }
        }

        public override long Position { get; set; }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _responseStream.Seek (offset, origin);
        }

        public override void SetLength(long length)
        {
            _responseStream.SetLength (length);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _responseStream.Read (buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            //all we are doing here is writing the contents of the buffer to the _response object
            //in the EndRequest event we retrieve this filter from the response and read the 
            //Response property to get the response body for storage.
            _response.Append(Encoding.UTF8.GetString(buffer, offset, count));
            _responseStream.Write(buffer, offset, count); 
        }

        public string Response
        {
            get { return _response.ToString(); }
        }
    }
}
