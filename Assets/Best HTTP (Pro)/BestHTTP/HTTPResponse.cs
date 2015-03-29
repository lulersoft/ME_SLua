using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if (!UNITY_WP8 && !UNITY_WINRT && !UNITY_METRO) || UNITY_EDITOR
using System.Net.Sockets;
#endif

namespace BestHTTP
{
    using BestHTTP.Caching;
    using BestHTTP.Cookies;
    using BestHTTP.Extensions;

    /// <summary>
    /// 
    /// </summary>
    public class HTTPResponse : IDisposable
    {
        internal const byte CR = 13;
        internal const byte LF = 10;

        internal const int BufferSize = 4 * 1024;

        #region Public Properties

        public int VersionMajor { get; protected set; }
        public int VersionMinor { get; protected set; }

        /// <summary>
        /// The status code that sent from the server.
        /// </summary>
        public int StatusCode { get; protected set; }

        /// <summary>
        /// Returns true if the status code is in the range of [200..300[ or 304 (Not Modified)
        /// </summary>
        public bool IsSuccess { get { return (this.StatusCode >= 200 && this.StatusCode < 300) || this.StatusCode == 304; } }

        /// <summary>
        /// The message that sent along with the StatusCode from the server. You can check it for errors from the server.
        /// </summary>
        public string Message { get; protected set; }

        /// <summary>
        /// True if it's a streamed response.
        /// </summary>
        public bool IsStreamed { get; protected set; }

        /// <summary>
        /// True if the streaming is finished, and no more fragments are coming.
        /// </summary>
        public bool IsStreamingFinished { get; internal set; }

        /// <summary>
        /// Indicates that the response body is read from the cache.
        /// </summary>
        public bool IsFromCache { get; internal set; }

        /// <summary>
        /// The headers that sent from the server.
        /// </summary>
        public Dictionary<string, List<string>> Headers { get; protected set; }

        /// <summary>
        /// The data that donwloaded from the server. All Transfer and Content encodings decoded if any(eg. chunked, gzip, deflate).
        /// </summary>
        public byte[] Data { get; internal set; }

        /// <summary>
        /// The normal HTTP protocol is upgraded to an other.
        /// </summary>
        public bool IsUpgraded { get; protected set; }

        /// <summary>
        /// The cookies that the server sent to the client.
        /// </summary>
        public List<Cookie> Cookies { get; internal set; }

        /// <summary>
        /// Cached, converted data.
        /// </summary>
        protected string dataAsText;

        /// <summary>
        /// The data converted to an UTF8 string.
        /// </summary>
        public string DataAsText
        {
            get
            {
                if (Data == null)
                    return string.Empty;

                if (!string.IsNullOrEmpty(dataAsText))
                    return dataAsText;

                return dataAsText = Encoding.UTF8.GetString(Data, 0, Data.Length);
            }
        }

        /// <summary>
        /// Cached converted data.
        /// </summary>
        protected UnityEngine.Texture2D texture;

        /// <summary>
        /// The data loaded to a Texture2D.
        /// </summary>
        public UnityEngine.Texture2D DataAsTexture2D
        {
            get
            {
                if (Data == null)
                    return null;

                if (texture != null)
                    return texture;

                texture = new UnityEngine.Texture2D(0, 0);
                texture.LoadImage(Data);
                return texture;
            }
        }

        #endregion

        #region Internal Fields

        internal HTTPRequest baseRequest;

        #endregion

        #region Protected Properties And Fields

        protected Stream Stream;

        protected List<byte[]> streamedFragments;
        protected object SyncRoot = new object();

        protected byte[] fragmentBuffer;
        protected int fragmentBufferDataLength;
        protected Stream cacheStream;
        protected int allFragmentSize;

        #endregion

        internal HTTPResponse(HTTPRequest request, Stream stream, bool isStreamed, bool isFromCache)
        {
            this.baseRequest = request;
            this.Stream = stream;
            this.IsStreamed = isStreamed;
            this.IsFromCache = isFromCache;
        }

        internal virtual bool Receive(int forceReadRawContentLength = -1)
        {
            string versionStr = string.Empty;

            // On WP platform we aren't able to determined sure enough whether the tcp connection is closed or not.
            //  So if we get an exception here, we need to recreate the connection.
            try
            {
                // Read out 'HTTP/1.1' from the "HTTP/1.1 {StatusCode} {Message}"
                versionStr = ReadTo(Stream, (byte)' ');
            }
            catch (Exception e)
            {
                if (!baseRequest.DisableRetry)
                    return false;

                throw e;
            }

            if (!baseRequest.DisableRetry && string.IsNullOrEmpty(versionStr))
                return false;

            string[] versions = versionStr.Split(new char[] { '/', '.' });
            this.VersionMajor = int.Parse(versions[1]);
            this.VersionMinor = int.Parse(versions[2]);

            int statusCode;
            string statusCodeStr = ReadTo(Stream, (byte)' ');

            if (baseRequest.DisableRetry)
                statusCode = int.Parse(statusCodeStr);
            else if (!int.TryParse(statusCodeStr, out statusCode))
                return false;

            this.StatusCode = statusCode;

            this.Message = ReadTo(Stream, LF);

            //Read Headers 
            ReadHeaders(Stream);

            IsUpgraded = StatusCode == 101 && HasHeaderWithValue("connection", "upgrade");

            // Reading from an already unpacked stream (eq. From a file cache)
            if (forceReadRawContentLength != -1)
            {
                this.IsFromCache = true;
                ReadRaw(Stream, forceReadRawContentLength);

                return true;
            }

            //  http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.4
            //  1.Any response message which "MUST NOT" include a message-body (such as the 1xx, 204, and 304 responses and any response to a HEAD request)
            //      is always terminated by the first empty line after the header fields, regardless of the entity-header fields present in the message.
            if ((StatusCode >= 100 && StatusCode < 200) || StatusCode == 204 || StatusCode == 304 || baseRequest.MethodType == HTTPMethods.Head)
                return true;

            if (HasHeaderWithValue("transfer-encoding", "chunked"))
                ReadChunked(Stream);
            else
            {
                //  http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.4
                //      Case 3 in the above link.
                List<string> contentLengthHeaders = GetHeaderValues("content-length");
                var contentRangeHeaders = GetHeaderValues("content-range");
                if (contentLengthHeaders != null && contentRangeHeaders == null)
                    ReadRaw(Stream, int.Parse(contentLengthHeaders[0]));
                else if (contentRangeHeaders != null)
                {
                    HTTPRange range = GetRange();
                    ReadRaw(Stream, (range.LastBytePos - range.FirstBytePos) + 1);
                }
                else
                    ReadUnknownSize(Stream);
            }

            return true;
        }

        #region Header Management

        protected void ReadHeaders(Stream stream)
        {
            string headerName = ReadTo(stream, (byte)':', LF).Trim();
            while (headerName != string.Empty)
            {
                string value = ReadTo(stream, LF);

                AddHeader(headerName, value);

                headerName = ReadTo(stream, (byte)':', LF);
            }
        }

        protected void AddHeader(string name, string value)
        {
            name = name.ToLower();

            if (Headers == null)
                Headers = new Dictionary<string, List<string>>();

            List<string> values;
            if (!Headers.TryGetValue(name, out values))
                Headers.Add(name, values = new List<string>(1));

            values.Add(value);
        }

        /// <summary>
        /// Returns the list of values that received from the server for the given header name.
        /// <remarks>Remarks: All headers converted to lowercase while reading the response.</remarks>
        /// </summary>
        /// <param name="name">Name of the header</param>
        /// <returns>If no header found with the given name or there are no values in the list (eg. Count == 0) returns null.</returns>
        public List<string> GetHeaderValues(string name)
        {
            name = name.ToLower();

            List<string> values;
            if (!Headers.TryGetValue(name, out values) || values.Count == 0)
                return null;

            return values;
        }

        /// <summary>
        /// Returns the first value in the header list or null if there are no header or value.
        /// </summary>
        /// <param name="name">Name of the header</param>
        /// <returns>If no header found with the given name or there are no values in the list (eg. Count == 0) returns null.</returns>
        public string GetFirstHeaderValue(string name)
        {
            name = name.ToLower();

            List<string> values;
            if (!Headers.TryGetValue(name, out values) || values.Count == 0)
                return null;

            return values[0];
        }

        /// <summary>
        /// Checks if there is a header with the given name and value.
        /// </summary>
        /// <param name="headerName">Name of the header.</param>
        /// <param name="value"></param>
        /// <returns>Returns true if there is a header with the given name and value.</returns>
        public bool HasHeaderWithValue(string headerName, string value)
        {
            var values = GetHeaderValues(headerName);
            if (values == null)
                return false;

            for (int i = 0; i < values.Count; ++i)
                if (string.Compare(values[i], value, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Checks if there is a header with the given name.
        /// </summary>
        /// <param name="headerName">Name of the header.</param>
        /// <returns>Returns true if there is a header with the given name.</returns>
        public bool HasHeader(string headerName)
        {
            var values = GetHeaderValues(headerName);
            if (values == null)
                return false;

            return true;
        }

        /// <summary>
        /// Parses the 'Content-Range' header's value and returns a HTTPRange object.
        /// </summary>
        /// <remarks>If the server ignores a byte-range-spec because it is syntactically invalid, the server SHOULD treat the request as if the invalid Range header field did not exist.
        /// (Normally, this means return a 200 response containing the full entity). In this case becouse of there are no 'Content-Range' header, this function will return null!</remarks>
        /// <returns>Returns null if no 'Content-Range' header found.</returns>
        public HTTPRange GetRange()
        {
            var rangeHeaders = GetHeaderValues("content-range");
            if (rangeHeaders == null)
                throw null;

            // A byte-content-range-spec with a byte-range-resp-spec whose last- byte-pos value is less than its first-byte-pos value, 
            //  or whose instance-length value is less than or equal to its last-byte-pos value, is invalid.
            // The recipient of an invalid byte-content-range- spec MUST ignore it and any content transferred along with it.

            // A valid content-range sample: "bytes 500-1233/1234"
            var ranges = rangeHeaders[0].Split(new char[] { ' ', '-', '/' }, StringSplitOptions.RemoveEmptyEntries);

            // A server sending a response with status code 416 (Requested range not satisfiable) SHOULD include a Content-Range field with a byte-range-resp-spec of "*".
            // The instance-length specifies the current length of the selected resource.
            // "bytes */1234"
            if (ranges[1] == "*")
                return new HTTPRange(int.Parse(ranges[2]));

            return new HTTPRange(int.Parse(ranges[1]), int.Parse(ranges[2]), ranges[3] != "*" ? int.Parse(ranges[3]) : -1);
        }

        #endregion

        #region Stream Management

        protected string ReadTo(Stream stream, byte blocker)
        {
            using (var ms = new MemoryStream())
            {
                int ch = stream.ReadByte();
                while (ch != blocker && ch != -1)
                {
                    ms.WriteByte((byte)ch);
                    ch = stream.ReadByte();
                }

                return ms.ToArray().AsciiToString().Trim();
            }
        }

        protected string ReadTo(Stream stream, byte blocker1, byte blocker2)
        {
            using (var ms = new MemoryStream())
            {
                int ch = stream.ReadByte();
                while (ch != blocker1 && ch != blocker2 && ch != -1)
                {
                    ms.WriteByte((byte)ch);
                    ch = stream.ReadByte();
                }

                return ms.ToArray().AsciiToString().Trim();
            }
        }

        #endregion

        #region Read Chunked Body

        protected int ReadChunkLength(Stream stream)
        {
            // Read until the end of line, then split the string so we will discard any optional chunk extensions
            return int.Parse(ReadTo(stream, LF).Split(';')[0], System.Globalization.NumberStyles.AllowHexSpecifier);
        }

        // http://www.w3.org/Protocols/rfc2616/rfc2616-sec3.html#sec3.6.1
        protected void ReadChunked(Stream stream)
        {
            BeginReceiveStreamFragments();

            using (var output = new MemoryStream())
            {
                int chunkLength = ReadChunkLength(stream);
                byte[] buffer = new byte[chunkLength];

                int contentLength = 0;

                // Progress report:
                baseRequest.DownloadLength = chunkLength;
                baseRequest.DownloadProgressChanged = this.IsSuccess || this.IsFromCache;

                while (chunkLength != 0)
                {
                    // To avoid more GC garbage we use only one buffer, and resize only if the next chunk doesn't fit.
                    if (buffer.Length < chunkLength)
                        Array.Resize<byte>(ref buffer, chunkLength);

                    int readBytes = 0;

                    // If reading from cache, we don't want to read too much data to memory. So we will wait until the loaded fragment
                    //  doesn't processed.
                    WaitWhileHasFragments();

                    // Fill up the buffer
                    do
                    {
                        readBytes += stream.Read(buffer, readBytes, chunkLength - readBytes);
                    } while (readBytes < chunkLength);

                    if (baseRequest.UseStreaming)
                        FeedStreamFragment(buffer, 0, readBytes);
                    else
                        output.Write(buffer, 0, readBytes);

                    // Every chunk data has a trailing CRLF 
                    ReadTo(stream, LF);

                    contentLength += readBytes;

                    // read the next chunk's length
                    chunkLength = ReadChunkLength(stream);

                    // Progress report:
                    baseRequest.DownloadLength += chunkLength;
                    baseRequest.Downloaded = contentLength;
                    baseRequest.DownloadProgressChanged = this.IsSuccess || this.IsFromCache;
                }

                if (baseRequest.UseStreaming)
                    FlushRemainingFragmentBuffer();

                // Read the trailing headers or the CRLF
                ReadHeaders(stream);

                // HTTP servers sometimes use compression (gzip) or deflate methods to optimize transmission. 
                // How both chunked and gzip encoding interact is dictated by the two-staged encoding of HTTP: 
                //  first the content stream is encoded as (Content-Encoding: gzip), after which the resulting byte stream is encoded for transfer using another encoder (Transfer-Encoding: chunked).
                //  This means that in case both compression and chunked encoding are enabled, the chunk encoding itself is not compressed, and the data in each chunk should not be compressed individually.
                //  The remote endpoint can decode the incoming stream by first decoding it with the Transfer-Encoding, followed by the specified Content-Encoding.
                // It would be a better implementation when the chunk would be decododed on-the-fly. Becouse now the whole stream must be downloaded, and then decoded. It needs more memory.
                if (!baseRequest.UseStreaming)
                    this.Data = DecodeStream(output);
            }
        }

        #endregion

        #region Read Raw Body

        // No transfer-encoding just raw bytes.
        internal void ReadRaw(Stream stream, int contentLength)
        {
            BeginReceiveStreamFragments();

            // Progress report:
            baseRequest.DownloadLength = contentLength;
            baseRequest.DownloadProgressChanged = this.IsSuccess || this.IsFromCache;

            using (var output = new MemoryStream(baseRequest.UseStreaming ? 0 : contentLength))
            {
                byte[] buffer = new byte[Math.Min(baseRequest.StreamFragmentSize, 4096)];
                int readBytes = 0;

                while (contentLength > 0)
                {
                    readBytes = 0;

                    // If reading from cache, we don't want to read to much data to memory. So we will wait until the loaded fragment
                    //  doesn't processed.
                    WaitWhileHasFragments();

                    do
                    {
                        int bytes = stream.Read(buffer, readBytes, Math.Min(contentLength, buffer.Length - readBytes));
                        readBytes += bytes;
                        contentLength -= bytes;

                        // Progress report:
                        baseRequest.Downloaded += bytes;
                        baseRequest.DownloadProgressChanged = this.IsSuccess || this.IsFromCache;

                    } while (readBytes < buffer.Length && contentLength > 0);

                    if (baseRequest.UseStreaming)
                        FeedStreamFragment(buffer, 0, readBytes);
                    else
                        output.Write(buffer, 0, readBytes);
                };

                if (baseRequest.UseStreaming)
                    FlushRemainingFragmentBuffer();

                if (!baseRequest.UseStreaming)
                    this.Data = DecodeStream(output);
            }
        }

        #endregion

        #region Read Unknown Size

        private void ReadUnknownSize(Stream stream)
        {
#if (!UNITY_WP8 && !UNITY_WINRT && !UNITY_METRO) || UNITY_EDITOR
            NetworkStream networkStream = stream as NetworkStream;
#endif

            using (var output = new MemoryStream())
            {
                byte[] buffer = new byte[Math.Min(baseRequest.StreamFragmentSize, 4096)];
                int readBytes = 0;
                int bytes = 0;
                do
                {
                    readBytes = 0;

                    do
                    {
                        bytes = 0;

#if (!UNITY_WP8 && !UNITY_WINRT && !UNITY_METRO) || UNITY_EDITOR
                        // If we have the good-old NetworkStream, than we can use the DataAvailable property. On WP8 platforms, these are omitted... :/
                        if (networkStream != null)
                        {
                            for (int i = readBytes; i < buffer.Length && networkStream.DataAvailable; ++i)
                            {
                                int read = stream.ReadByte();
                                if (read >= 0)
                                {
                                    buffer[i] = (byte)read;
                                    bytes++;
                                }
                                else
                                    break;
                            }
                        }
                        else // This will be good anyway, but a little slower.
#endif
                            bytes = stream.Read(buffer, readBytes, buffer.Length - readBytes);

                        readBytes += bytes;

                        // Progress report:
                        baseRequest.Downloaded += bytes;
                        baseRequest.DownloadLength = baseRequest.Downloaded;
                        baseRequest.DownloadProgressChanged = this.IsSuccess || this.IsFromCache;

                    } while (readBytes < buffer.Length && bytes > 0);

                    if (baseRequest.UseStreaming)
                        FeedStreamFragment(buffer, 0, readBytes);
                    else
                        output.Write(buffer, 0, readBytes);

                } while (bytes > 0);

                if (baseRequest.UseStreaming)
                    FlushRemainingFragmentBuffer();

                if (!baseRequest.UseStreaming)
                    this.Data = DecodeStream(output);
            }
        }

        #endregion

        #region Stream Decoding

        protected byte[] DecodeStream(Stream streamToDecode)
        {
            streamToDecode.Seek(0, SeekOrigin.Begin);

            // The cache stores the decoded data
            var encoding = IsFromCache ? null : GetHeaderValues("content-encoding");

            Stream decoderStream = null;
            if (encoding == null)
                decoderStream = streamToDecode;
            else
            {
                switch (encoding[0])
                {
                    case "gzip": decoderStream = new Decompression.Zlib.GZipStream(streamToDecode, Decompression.Zlib.CompressionMode.Decompress); break;
                    case "deflate": decoderStream = new Decompression.Zlib.DeflateStream(streamToDecode, Decompression.Zlib.CompressionMode.Decompress); break;
                    default:
                        //identity, utf-8, etc.
                        decoderStream = streamToDecode;
                        break;
                }
            }

            using (var ms = new MemoryStream((int)streamToDecode.Length))
            {
                var buf = new byte[1024];
                int byteCount = 0;

                while ((byteCount = decoderStream.Read(buf, 0, buf.Length)) > 0)
                    ms.Write(buf, 0, byteCount);

                return ms.ToArray();
            }
        }

        #endregion

        #region Streaming Fragments Support

        protected void BeginReceiveStreamFragments()
        {
            if (!baseRequest.DisableCache && baseRequest.UseStreaming)
            {
                // If caching is enabled and the response not from cache and it's cacheble the we will cache the downloaded data.
                if (!IsFromCache && HTTPCacheService.IsCacheble(baseRequest.CurrentUri, baseRequest.MethodType, this))
                    cacheStream = HTTPCacheService.PrepareStreamed(baseRequest.CurrentUri, this);
            }
            allFragmentSize = 0;
        }

        /// <summary>
        /// Add data to the fragments list.
        /// </summary>
        /// <param name="buffer">The buffer to be added.</param>
        /// <param name="pos">The position where we start copy the data.</param>
        /// <param name="length">How many data we want to copy.</param>
        protected void FeedStreamFragment(byte[] buffer, int pos, int length)
        {
            if (fragmentBuffer == null)
            {
                fragmentBuffer = new byte[baseRequest.StreamFragmentSize];
                fragmentBufferDataLength = 0;
            }

            if (fragmentBufferDataLength + length <= baseRequest.StreamFragmentSize)
            {
                Array.Copy(buffer, pos, fragmentBuffer, fragmentBufferDataLength, length);
                fragmentBufferDataLength += length;

                if (fragmentBufferDataLength == baseRequest.StreamFragmentSize)
                {
                    AddStreamedFragment(fragmentBuffer);
                    fragmentBuffer = null;
                    fragmentBufferDataLength = 0;
                }
            }
            else
            {
                int remaining = baseRequest.StreamFragmentSize - fragmentBufferDataLength;

                FeedStreamFragment(buffer, pos, remaining);
                FeedStreamFragment(buffer, pos + remaining, length - remaining);
            }
        }

        protected void FlushRemainingFragmentBuffer()
        {
            if (fragmentBuffer != null)
            {
                Array.Resize<byte>(ref fragmentBuffer, fragmentBufferDataLength);

                AddStreamedFragment(fragmentBuffer);
                fragmentBuffer = null;
                fragmentBufferDataLength = 0;
            }

            if (cacheStream != null)
            {
                cacheStream.Dispose();
                cacheStream = null;

                HTTPCacheService.SetBodyLength(baseRequest.CurrentUri, allFragmentSize);
            }
        }

        protected void AddStreamedFragment(byte[] buffer)
        {
            lock (SyncRoot)
            {
                if (streamedFragments == null)
                    streamedFragments = new List<byte[]>();
                streamedFragments.Add(buffer);

                if (cacheStream != null)
                {
                    cacheStream.Write(buffer, 0, buffer.Length);
                    allFragmentSize += buffer.Length;
                }
            }
        }

        protected void WaitWhileHasFragments()
        {
            // TODO: use this when only the data loaded from cache too
            // TODO: Test it out before releasing
            /*if (baseRequest.UseStreaming && this.IsFromCache)
                while (HasStreamedFragments())
                    System.Threading.Thread.Sleep(10);*/
        }

        /// <summary>
        /// If streaming is used, then every time this callback function called we can use this function to
        ///  retrive the downloaded and buffered data. The returned list can be null, if there is no data yet.
        /// </summary>
        /// <returns></returns>
        public List<byte[]> GetStreamedFragments()
        {
            lock (SyncRoot)
            {
                if (streamedFragments == null || streamedFragments.Count == 0)
                    return null;
                
                var result = new List<byte[]>(streamedFragments);
                streamedFragments.Clear();

                return result;
            }
        }

        internal bool HasStreamedFragments()
        {
            lock (SyncRoot)
                return streamedFragments != null && streamedFragments.Count > 0;
        }

        internal void FinishStreaming()
        {
            IsStreamingFinished = true;
            Dispose();
        }

        #endregion

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose()
        {
            if (cacheStream != null)
            {
                cacheStream.Dispose();
                cacheStream = null;
            }
        }
    }
}