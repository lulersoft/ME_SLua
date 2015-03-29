using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BestHTTP
{
    using BestHTTP.Authentication;
    using BestHTTP.Cookies;
    using BestHTTP.Extensions;
    using BestHTTP.Forms;

    /// <summary>
    /// 
    /// </summary>
    public sealed class HTTPRequest : System.Collections.IEnumerator
    {
        internal static readonly byte[] EOL = { HTTPResponse.CR, HTTPResponse.LF };

        #region Properties

        /// <summary>
        /// The original request's Uri.
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// The method that how we want to process our request the server.
        /// </summary>
        public HTTPMethods MethodType { get; private set; }

        /// <summary>
        /// The raw data to send in a POST request. If it set all other fields that added to this request will be ignored.
        /// </summary>
        public byte[] RawData { get; set; }

        /// <summary>
        /// Indicates that the connection should be open after the response received. If its true, then the internal tcp connections will be reused if it's possible. Default value is true.
        /// The default value can be changed in the HTTPManager class. If you make rare request to the server it's should be changed to false.
        /// </summary>
        public bool IsKeepAlive
        {
            get { return isKeepAlive; }
            set
            {
                if (State == HTTPRequestStates.Processing)
                    throw new NotSupportedException("Changing the IsKeepAlive property while processing the request is not supported.");
                isKeepAlive = value;
            }
        }

        /// <summary>
        /// With this property caching can be enabled/disabled on a per-request basis.
        /// </summary>
        public bool DisableCache
        {
            get { return disableCache; }
            set
            {
#if !UNITY_WEBPLAYER
                if (State == HTTPRequestStates.Processing)
                    throw new NotSupportedException("Changing the DisableCache property while processing the request is not supported.");
#endif
                disableCache = value;
            }
        }

        /// <summary>
        /// If it's true, the callback function will be called every time if we can send out at least one fragment 
        /// </summary>
        public bool UseStreaming
        {
            get { return useStreaming; }
            set
            {
                if (State == HTTPRequestStates.Processing)
                    throw new NotSupportedException("Changing the UseStreaming property while processing the request is not supported.");
                useStreaming = value;
            }
        }

        /// <summary>
        /// Maximum size of a data chunk that we want to receive when streaming is set.
        /// </summary>
        public int StreamFragmentSize
        {
            get{ return streamFragmentSize; }
            set
            {
                if (State == HTTPRequestStates.Processing)
                    throw new NotSupportedException("Changing the StreamFragmentSize property while processing the request is not supported.");

                if (value < 1)
                    throw new System.ArgumentException("StreamFragmentSize must be at least 1.");

                streamFragmentSize = value;
            }
        }

        /// <summary>
        /// The callback function that will be called when a request is fully processed or when any downloaded fragment is available if UseStreaming is true. Can be null for fire-and-forget requests.
        /// </summary>
        public Action<HTTPRequest, HTTPResponse> Callback
        {
            get { return callback; }
            set
            {
                if (State == HTTPRequestStates.Processing)
                    throw new NotSupportedException("Changing the StreamFragmentSize property while processing the request is not supported.");
                callback = value;
            }
        }

        /// <summary>
        /// Called when new data downloaded from the server.
        /// The first parameter is the original HTTTPRequest object itself, the secound parameter is the downloaded bytes while the third parameter is the content length.
        /// <remarks>There are download modes where we can't figure out the exact length of the final content. In these cases we just guarantee that the third parameter will be at least the size of the secound one.</remarks>
        /// </summary>
        public Action<HTTPRequest, int, int> OnProgress;

        /// <summary>
        /// Called when the current protocol is upgraded to an other. (HTTP => WebSocket for example)
        /// </summary>
        public Action<HTTPRequest, HTTPResponse> OnUpgraded;

        /// <summary>
        /// With this option if reading back the server's response fails, the request will fail and any exceptions can be checked through the Exception property. The default value is True for POST requests, otherwise false.
        /// </summary>
        public bool DisableRetry { get; set; }

        /// <summary>
        /// Indicates that the request is redirected. If a request is redirected, the connection that served it will be closed regardless of the value of IsKeepAlive.
        /// </summary>
        public bool IsRedirected { get; internal set; }

        /// <summary>
        /// The Uri that the request redirected to.
        /// </summary>
        public Uri RedirectUri { get; internal set; }

        /// <summary>
        /// If redirected it's contains the RedirectUri.
        /// </summary>
        public Uri CurrentUri { get { return IsRedirected ? RedirectUri : Uri; } }

        /// <summary>
        /// The response to the query.
        /// <remarks>If an exception occured during reading of the response stream or can't connect to the server, this will be null!</remarks>
        /// </summary>
        public HTTPResponse Response { get; internal set; }

        /// <summary>
        /// It there is an exception while processing the request or response the Response property will be null, and the Exception will be stored in this property.
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// Any object can be passed with the request with this property. (eq. it can be identified, etc.)
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// The UserName, Password pair that the plugin will use to authenticate to the remote server.
        /// </summary>
        public Credentials Credentials { get; set; }

        /// <summary>
        /// True, if there is a Proxy object.
        /// </summary>
        public bool HasProxy { get { return Proxy != null; } }

        /// <summary>
        /// A web proxy's properties where the request must pass through.
        /// </summary>
        public HTTPProxy Proxy { get; set; }

        /// <summary>
        /// How many redirection supported for this request. The default is int.MaxValue. 0 or a negative value means no redirection supported.
        /// </summary>
        public int MaxRedirects { get; set; }

        /// <summary>
        /// Use Bouncy Castle's code to handle the secure protocol instead of Mono's. You can try to set it true if you receive a "System.Security.Cryptography.CryptographicException: Unsupported hash algorithm" exception.
        /// </summary>
        public bool UseAlternateSSL { get; set; }

        /// <summary>
        /// If true cookies will be added to the headers (if any), and parsed from the response. If false, all cookie operations will be ignored. It's default value is HTTPManager's IsCookiesEnabled.
        /// </summary>
        public bool IsCookiesEnabled { get; set; }

        /// <summary>
        /// What form should used. Default to Automatic.
        /// </summary>
        public HTTPFormUsage FormUsage { get; set; }

        /// <summary>
        /// Current state of this request.
        /// </summary>
        public HTTPRequestStates State { get; internal set; }

        /// <summary>
        /// How many times redirected.
        /// </summary>
        public int RedirectCount { get; internal set; }

#if !UNITY_WP8 && !NETFX_CORE
        /// <summary>
        /// Custom validator for an SslStream. This event will receive the original HTTPRequest, an X509Certificate and an X509Chain objects. It must return true if the certificate valid, false otherwise.
        /// <remarks>It's called in a thread! Not available on Windows Phone!</remarks>
        /// </summary>
        public event System.Func<HTTPRequest, System.Security.Cryptography.X509Certificates.X509Certificate, System.Security.Cryptography.X509Certificates.X509Chain, bool> CustomCertificationValidator;
#endif

        /// <summary>
        /// Maximum time we wait to estabilish the connection to the target server. Default value is 20 seconds.
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; }

        /// <summary>
        /// Maximum time we want to wait to the request to finish after the connection is estabilished. Default value is 60 seconds.
        /// <remarks>It's disabled for streaming requests! See <see cref="EnableTimoutForStreaming"/>.</remarks>
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Set to true to enable Timeouts on streaming request. Default value is false.
        /// </summary>
        public bool EnableTimoutForStreaming { get; set; }

        #region Internal Properties For Progress Report Support

        /// <summary>
        /// How many bytes downloaded so far.
        /// </summary>
        internal int Downloaded { get; set; }

        /// <summary>
        /// The length of the content that we are currently downloading.
        /// If chunked encoding is used, then it is the size of the sum of all previous chunks plus the current one.
        /// When no Content-Length present and no chunked encoding is used then its size is the currently downloaded size.
        /// </summary>
        internal int DownloadLength { get; set; }

        /// <summary>
        /// Set to true when the downloaded bytes are changed, and set to false when the OnProgress event called.
        /// </summary>
        internal bool DownloadProgressChanged { get; set; }

        #endregion

        #endregion

        #region Privates

        private bool isKeepAlive;
        private bool disableCache;
        private int streamFragmentSize;
        private bool useStreaming;
        private Action<HTTPRequest, HTTPResponse> callback;

        private Dictionary<string, List<string>> Headers { get; set; }

        /// <summary>
        /// We will collect the fields and values to the FieldCollector through the AddField and AddBinaryData functions.
        /// </summary>
        private HTTPFormBase FieldCollector;

        /// <summary>
        /// When the request about to send the request we will create a specialised form implementation(url-encoded, multipart, or the legacy WWWForm based).
        /// And we will use this instance to create the data that we will send to the server.
        /// </summary>
        private HTTPFormBase FormImpl;

        #endregion

        #region Constructors

        #region Default Get Constructors

        public HTTPRequest(Uri uri)
            : this(uri, HTTPMethods.Get, HTTPManager.KeepAliveDefaultValue, HTTPManager.IsCachingDisabled, null)
        {
        }

        public HTTPRequest(Uri uri, Action<HTTPRequest, HTTPResponse> callback)
            : this(uri, HTTPMethods.Get, HTTPManager.KeepAliveDefaultValue, HTTPManager.IsCachingDisabled, callback)
        {
        }

        public HTTPRequest(Uri uri, bool isKeepAlive, Action<HTTPRequest, HTTPResponse> callback)
            : this(uri, HTTPMethods.Get, isKeepAlive, HTTPManager.IsCachingDisabled, callback)
        {
        }
        public HTTPRequest(Uri uri, bool isKeepAlive, bool disableCache, Action<HTTPRequest, HTTPResponse> callback)
            : this(uri, HTTPMethods.Get, isKeepAlive, disableCache, callback)
        {
        }

        #endregion

        public HTTPRequest(Uri uri, HTTPMethods methodType, Action<HTTPRequest, HTTPResponse> callback)
            : this(uri, methodType, HTTPManager.KeepAliveDefaultValue, HTTPManager.IsCachingDisabled, callback)
        {
        }

        public HTTPRequest(Uri uri, HTTPMethods methodType, bool isKeepAlive, Action<HTTPRequest, HTTPResponse> callback)
            : this(uri, methodType, isKeepAlive, HTTPManager.IsCachingDisabled, callback)
        {
        }

        public HTTPRequest(Uri uri, HTTPMethods methodType, bool isKeepAlive, bool disableCache, Action<HTTPRequest, HTTPResponse> callback)
        {
            this.Uri = uri;
            this.MethodType = methodType;
            this.IsKeepAlive = isKeepAlive;
            this.DisableCache = disableCache;
            this.Callback = callback;
            this.StreamFragmentSize = 4 * 1024;

            this.DisableRetry = methodType == HTTPMethods.Post;
            this.MaxRedirects = int.MaxValue;
            this.RedirectCount = 0;
            this.IsCookiesEnabled = HTTPManager.IsCookiesEnabled;

            Downloaded = DownloadLength = 0;
            DownloadProgressChanged = false;

            State = HTTPRequestStates.Initial;

            ConnectTimeout = HTTPManager.ConnectTimeout;
            Timeout = HTTPManager.RequestTimeout;
            EnableTimoutForStreaming = false;
        }

        #endregion

        #region Public Field Functions

        /// <summary>
        /// Add a field with a given string value.
        /// </summary>
        public void AddField(string fieldName, string value)
        {
            AddField(fieldName, value, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Add a field with a given string value.
        /// </summary>
        public void AddField(string fieldName, string value, System.Text.Encoding e)
        {
            if (FieldCollector == null)
                FieldCollector = new HTTPFormBase();

            FieldCollector.AddField(fieldName, value, e);
        }

        /// <summary>
        /// Add a field with binary content to the form.
        /// </summary>
        public void AddBinaryData(string fieldName, byte[] content)
        {
            AddBinaryData(fieldName, content, null, null);
        }

        /// <summary>
        /// Add a field with binary content to the form.
        /// </summary>
        public void AddBinaryData(string fieldName, byte[] content, string fileName)
        {
            AddBinaryData(fieldName, content, fileName, null);
        }

        /// <summary>
        /// Add a field with binary content to the form.
        /// </summary>
        public void AddBinaryData(string fieldName, byte[] content, string fileName, string mimeType)
        {
            if (FieldCollector == null)
                FieldCollector = new HTTPFormBase();

            FieldCollector.AddBinaryData(fieldName, content, fileName, mimeType);
        }

        /// <summary>
        /// Set or overwrite the internal form. Remarks: on WP8 it doesn't supported!
        /// </summary>
        public void SetFields(UnityEngine.WWWForm wwwForm)
        {
#if !UNITY_WP8
            FormUsage = HTTPFormUsage.Unity;
            FormImpl = new UnityForm(wwwForm);
#endif
        }

        /// <summary>
        /// Manually set a HTTP Form.
        /// </summary>
        public void SetForm(HTTPFormBase form)
        {
            FormImpl = form;
        }

        /// <summary>
        /// Will create the form implementation based on the value of the FormUsage property.
        /// </summary>
        private HTTPFormBase SelectFormImplementation()
        {
            // Our form already created with a previous
            if (FormImpl != null)
                return FormImpl;

            // No field added to this request yet
            if (FieldCollector == null)
                return null;

            switch (FormUsage)
            {
                case HTTPFormUsage.Automatic:
                    // A really simple decision making: if there are at least one field with binary data, or a 'long' string value then we will choose a Multipart form.
                    //  Otherwise Url Encoded form will be used.
                    if (FieldCollector.HasBinary || FieldCollector.HasLongValue)
                        goto case HTTPFormUsage.Multipart;
                    else
                        goto case HTTPFormUsage.UrlEncoded;

                case HTTPFormUsage.UrlEncoded:  FormImpl = new HTTPUrlEncodedForm(); break;
                case HTTPFormUsage.Multipart:   FormImpl = new HTTPMultiPartForm(); break;
#if !UNITY_WP8
                case HTTPFormUsage.Unity:       FormImpl = new UnityForm(); break;
#endif
            }

            // Copy the fields, and other properties to the new implementation
            FormImpl.CopyFrom(FieldCollector);

            return FormImpl;
        }

        #endregion

        #region Header Management

        #region General Management

        /// <summary>
        /// Adds a header and value pair to the Headers. Use it to add custom headers to the request.
        /// </summary>
        /// <example>AddHeader("User-Agent', "FooBar 1.0")</example>
        public void AddHeader(string name, string value)
        {
            if (Headers == null)
                Headers = new Dictionary<string, List<string>>();

            List<string> values;
            if (!Headers.TryGetValue(name, out values))
                Headers.Add(name, values = new List<string>(1));

            values.Add(value);
        }

        /// <summary>
        /// Removes any previously added values, and sets the given one.
        /// </summary>
        public void SetHeader(string name, string value)
        {
            if (Headers == null)
                Headers = new Dictionary<string, List<string>>();

            List<string> values;
            if (!Headers.TryGetValue(name, out values))
                Headers.Add(name, values = new List<string>(1));

            values.Clear();
            values.Add(value);
        }

        /// <summary>
        /// Removes the specified header. Returns true, if the header found and succesfully removed.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool RemoveHeader(string name)
        {
            if (Headers == null)
                return false;

            return Headers.Remove(name);
        }

        /// <summary>
        /// Returns true if the given head name is already in the Headers.
        /// </summary>
        public bool HasHeader(string name)
        {
            return Headers != null && Headers.ContainsKey(name);
        }

        /// <summary>
        /// Returns the first header or null for the given header name.
        /// </summary>
        public string GetFirstHeaderValue(string name)
        {
            if (Headers == null)
                return null;

            List<string> headers = null;
            if (Headers.TryGetValue(name, out headers) && headers.Count > 0)
                return headers[0];

            return null;
        }

        /// <summary>
        /// Returns all header values for the given header or null.
        /// </summary>
        public List<string> GetHeaderValues(string name)
        {
            if (Headers == null)
                return null;

            List<string> headers = null;
            if (Headers.TryGetValue(name, out headers) && headers.Count > 0)
                return headers;

            return null;
        }

        #endregion

        #region Range Headers

        /// <summary>
        /// Sets the Range header to download the content from the given byte position. See http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.35
        /// </summary>
        /// <param name="firstBytePos">Start position of the download.</param>
        public void SetRangeHeader(int firstBytePos)
        {
            SetHeader("Range", string.Format("bytes={0}-", firstBytePos));
        }

        /// <summary>
        /// Sets the Range header to download the content from the given byte position to the given last position. See http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.35
        /// </summary>
        /// <param name="firstBytePos">Start position of the download.</param>
        /// <param name="lastBytePos">The end position of the download.</param>
        public void SetRangeHeader(int firstBytePos, int lastBytePos)
        {
            SetHeader("Range", string.Format("bytes={0}-{1}", firstBytePos, lastBytePos));
        }

        #endregion

        /// <summary>
        /// Writes out the Headers to the stream.
        /// </summary>
        /// <param name="stream"></param>
        private void SendHeaders(BinaryWriter stream)
        {
            if (!HasHeader("Host"))
                SetHeader("Host", CurrentUri.Host);

            if (IsRedirected && !HasHeader("Referer"))
                AddHeader("Referer", Uri.ToString());

            if (!HasHeader("Accept-Encoding"))
                AddHeader("Accept-Encoding", "gzip, identity");

            if (HasProxy && !HasHeader("Proxy-Connection"))
                AddHeader("Proxy-Connection", IsKeepAlive ? "Keep-Alive" : "Close");
            
            if (!HasHeader("Connection"))
                AddHeader("Connection", IsKeepAlive ? "Keep-Alive, TE" : "Close, TE");

            if (!HasHeader("TE"))
                AddHeader("TE", "identity");

            if (!HasHeader("User-Agent"))
                AddHeader("User-Agent", "BestHTTP");

            byte[] entityBody = GetEntityBody();
            int contentLength = entityBody != null ? entityBody.Length : 0;

            if (RawData == null && (FormImpl != null || (FieldCollector != null && !FieldCollector.IsEmpty)))
            {
                SelectFormImplementation();
                if (FormImpl != null)
                   FormImpl.PrepareRequest(this);
            }

            if (!HasHeader("Content-Length") && contentLength != 0)
                AddHeader("Content-Length", contentLength.ToString());

            // Proxy Authentication
            if (HasProxy && Proxy.Credentials != null)
            {
                switch (Proxy.Credentials.Type)
                {
                    case AuthenticationTypes.Basic:
                        // With Basic authentication we don't want to wait for a challange, we will send the hash with the first request
                        SetHeader("Proxy-Authorization", string.Concat("Basic ", Convert.ToBase64String(Encoding.UTF8.GetBytes(Credentials.UserName + ":" + Credentials.Password))));
                        break;

                    case AuthenticationTypes.Unknown:
                    case AuthenticationTypes.Digest:
                        var digest = DigestStore.Get(Proxy.Address);
                        if (digest != null)
                        {
                            string authentication = digest.GenerateResponseHeader(this, Proxy.Credentials);
                            if (!string.IsNullOrEmpty(authentication))
                                SetHeader("Proxy-Authorization", authentication);
                        }

                        break;
                }
            }

            // Server authentication
            if (Credentials != null)
            {
                switch (Credentials.Type)
                {
                    case AuthenticationTypes.Basic:
                        // With Basic authentication we don't want to wait for a challange, we will send the hash with the first request
                        SetHeader("Authorization", string.Concat("Basic ", Convert.ToBase64String(Encoding.UTF8.GetBytes(Credentials.UserName + ":" + Credentials.Password))));
                        break;

                    case AuthenticationTypes.Unknown:
                    case AuthenticationTypes.Digest:
                        var digest = DigestStore.Get(this.CurrentUri);
                        if (digest != null)
                        {
                            string authentication = digest.GenerateResponseHeader(this, Credentials);
                            if (!string.IsNullOrEmpty(authentication))
                                SetHeader("Authorization", authentication);
                        }

                        break;
                }
            }

            // Cookies
            if (IsCookiesEnabled)
            {
                var cookies = CookieJar.Get(CurrentUri);

                // http://tools.ietf.org/html/rfc6265#section-5.4
                //  -When the user agent generates an HTTP request, the user agent MUST NOT attach more than one Cookie header field.
                if (cookies != null && cookies.Count > 0)
                {
                    // TODO:
                    //   2. The user agent SHOULD sort the cookie-list in the following order:
                    //      *  Cookies with longer paths are listed before cookies with shorter paths.
                    //      *  Among cookies that have equal-length path fields, cookies with earlier creation-times are listed before cookies with later creation-times.

                    bool first = true;
                    string cookieStr = string.Empty;

                    foreach (var cookie in cookies)
                        if ((!cookie.IsSecure || (cookie.IsSecure && HTTPProtocolFactory.IsSecureProtocol(CurrentUri))) &&
                            (!cookie.IsHttpOnly || (cookie.IsHttpOnly && HTTPProtocolFactory.GetProtocolFromUri(CurrentUri) == SupportedProtocols.HTTP)))
                        {
                            if (!first)
                                cookieStr += "; ";
                            else
                                first = false;

                            cookieStr += cookie.ToString();

                            // 3. Update the last-access-time of each cookie in the cookie-list to the current date and time.
                            cookie.LastAccess = DateTime.UtcNow;
                        }

                    SetHeader("Cookie", cookieStr);
                }
            }

            // Write out the headers to the stream
            foreach (var kvp in Headers)
            {
                byte[] headerName = string.Concat(kvp.Key, ": ").GetASCIIBytes();

                for (int i = 0; i < kvp.Value.Count; ++i)
                {
                    stream.Write(headerName);
                    stream.Write(kvp.Value[i].GetASCIIBytes());
                    stream.Write(EOL);
                }
            }
        }

        /// <summary>
        /// Returns a string representation of the headers.
        /// </summary>
        public string DumpHeaders()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                SendHeaders(bw);
                return ms.ToArray().AsciiToString();
            }
        }

        #endregion

        #region Internal Helper Functions

        internal byte[] GetEntityBody()
        {
            if (RawData != null)
                return RawData;

            if (FormImpl != null || (FieldCollector != null && !FieldCollector.IsEmpty))
            {
                SelectFormImplementation();
                if (FormImpl != null)
                    return FormImpl.GetData();
            }

            return null;
        }

        internal bool SendOutTo(Stream stream)
        {
            bool success = false;
            try
            {
                var outStream = new BinaryWriter(stream);

                //Method names are always uppercase.
                outStream.Write(string.Format("{0} {1} HTTP/1.1", MethodType.ToString().ToUpper(), CurrentUri.PathAndQuery).GetASCIIBytes());
                outStream.Write(EOL);

                SendHeaders(outStream);

                outStream.Write(EOL);

                byte[] data = RawData;
                
                if (data == null && FormImpl != null)
                    data = FormImpl.GetData();

                if (data != null && data.Length > 0)
                    outStream.Write(data, 0, data.Length);

                outStream.Flush();

                success = true;
            }
            catch
            {}

            return success;
        }

        internal void UpgradeCallback()
        {
            if (Response == null || !Response.IsUpgraded)
                return;

            try
            {
                if (OnUpgraded != null)
                    OnUpgraded(this, Response);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(string.Format("{0}: {1}", e.Message, e.StackTrace));
            }
        }

        internal void CallCallback()
        {
            try
            {
                if (Callback != null)
                    Callback(this, Response);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(string.Format("{0}: {1}", e.Message, e.StackTrace));
            }
        }

        internal void FinishStreaming()
        {
            if (Response != null && UseStreaming)
                Response.FinishStreaming();
        }

        /// <summary>
        /// Called on Unity's main thread just before processing it.
        /// </summary>
        internal void Prepare()
        {
#if !UNITY_WP8
            if (FormUsage == HTTPFormUsage.Unity)
                SelectFormImplementation();
#endif
        }

#if !UNITY_WP8 && !NETFX_CORE
        internal bool CallCustomCertificationValidator(System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain)
        {
            if (CustomCertificationValidator != null)
                return CustomCertificationValidator(this, cert, chain);
            return true;
        }
#endif

        #endregion

        /// <summary>
        /// Starts processing the request.
        /// </summary>
        public void Send()
        {
            HTTPManager.SendRequest(this);
        }

        /// <summary>
        /// Aborts an already estabilished connection, so no further download or upload are done.
        /// </summary>
        public void Abort()
        {
            lock (HTTPManager.Locker)
            {
                // Get the parent connection
                var connection = HTTPManager.GetConnectionWith(this);
                if (connection == null)
                {
                    UnityEngine.Debug.LogWarning("Abort - No active connection found with this request! (The download may already finished?)");
                    return;
                }

                connection.Abort(HTTPConnectionStates.AbortRequested);
            }
        }

        #region System.Collections.IEnumerator implementation

        public object Current { get { return null; } }

        public bool MoveNext()
        {
            lock (HTTPManager.Locker)
            {
                HTTPConnection connection = HTTPManager.GetConnectionWith(this);

                return connection != null && connection.State <= HTTPConnectionStates.WaitForRecycle;
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}