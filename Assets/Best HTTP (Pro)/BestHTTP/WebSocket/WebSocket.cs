using System;
using System.Text;
using BestHTTP.Extensions;
using BestHTTP.WebSocket.Frames;

namespace BestHTTP.WebSocket
{
    public sealed class WebSocket
    {
        #region Properties

        /// <summary>
        /// The internal HTTPRequest object.
        /// </summary>
        public HTTPRequest InternalRequest { get; private set; }

        /// <summary>
        /// The connection to the WebSocket server is open.
        /// </summary>
        public bool IsOpen { get { return webSocket != null && !webSocket.IsClosed; } }

        /// <summary>
        /// Set to true to start a new thread to send Pings to the WebSocket server
        /// </summary>
        public bool StartPingThread { get; set; }

        /// <summary>
        /// The delay between two Pings in millisecs. Minimum value is 100, default is 1000.
        /// </summary>
        public int PingFrequency { get; set; }

        /// <summary>
        /// Called when the connection to the WebSocket server is estabilished.
        /// </summary>
        public Action<WebSocket> OnOpen;

        /// <summary>
        /// Called when a new textual message is received from the server.
        /// </summary>
        public Action<WebSocket, string> OnMessage;

        /// <summary>
        /// Called when a new binary message is received from the server.
        /// </summary>
        public Action<WebSocket, byte[]> OnBinary;

        /// <summary>
        /// Called when the server is closed.
        /// </summary>
        public Action<WebSocket, UInt16, string> OnClosed;

        /// <summary>
        /// Called when an error is encountered. The Exception parameter may be null.
        /// </summary>
        public Action<WebSocket, Exception> OnError;

        /// <summary>
        /// Called when an incomplete frame received. No attemp will be made to reassemble these fragments internally, and no reference are stored after this event to this frame.
        /// </summary>
        public Action<WebSocket, WebSocketFrameReader> OnIncompleteFrame;

        #endregion

        #region Private Fields

        /// <summary>
        /// Indicates wheter we sent out the connection request to the server.
        /// </summary>
        private bool requestSent;

        /// <summary>
        /// The internal WebSocketResponse object
        /// </summary>
        private WebSocketResponse webSocket;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a WebSocket instance from the given uri.
        /// </summary>
        /// <param name="uri">The uri of the WebSocket server</param>
        public WebSocket(Uri uri)
            :this(uri, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Creates a WebSocket instance from the given uri, protocol and origin.
        /// </summary>
        /// <param name="uri">The uri of the WebSocket server</param>
        /// <param name="origin">Servers that are not intended to process input from any web page but only for certain sites SHOULD verify the |Origin| field is an origin they expect. 
        /// If the origin indicated is unacceptable to the server, then it SHOULD respond to the WebSocket handshake with a reply containing HTTP 403 Forbidden status code.</param>
        /// <param name="protocol">The application-level protocol that the client want to use(eg. "chat", "leaderboard", etc.). Can be null or empty string if not used.</param>
        public WebSocket(Uri uri, string origin, string protocol = "")
        {
            // Set up some default values.
            this.PingFrequency = 1000;

            // If there no port set in the uri, we must set it now.
            if (uri.Port == -1)
                // Somehow if i use the UriBuilder its not the same as if the uri is constructed from a string...
                //uri = new UriBuilder(uri.Scheme, uri.Host, uri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase) ? 443 : 80, uri.PathAndQuery).Uri;
                uri = new Uri(uri.Scheme + "://" + uri.Host + ":" + (uri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase) ? "443" : "80") + uri.PathAndQuery);

            InternalRequest = new HTTPRequest(uri, (req, resp) => {
                    if ((resp == null || req.Exception != null) && OnError != null || (resp != null && resp.StatusCode != 101))
                        OnError(this, req.Exception);
                });

            //http://tools.ietf.org/html/rfc6455#section-4

            //The request MUST contain a |Host| header field whose value contains /host/ plus optionally ":" followed by /port/ (when not using the default port).
            InternalRequest.SetHeader("Host", uri.Host + ":" + uri.Port);

            // The request MUST contain an |Upgrade| header field whose value MUST include the "websocket" keyword.
            InternalRequest.SetHeader("Upgrade", "websocket");

            // The request MUST contain a |Connection| header field whose value MUST include the "Upgrade" token.
            InternalRequest.SetHeader("Connection", "keep-alive, Upgrade");

            // The request MUST include a header field with the name |Sec-WebSocket-Key|.  The value of this header field MUST be a nonce consisting of a 
            // randomly selected 16-byte value that has been base64-encoded (see Section 4 of [RFC4648]).  The nonce MUST be selected randomly for each connection.
            InternalRequest.SetHeader("Sec-WebSocket-Key", GetSecKey(new object[] { this, InternalRequest, uri, new object() }));

            // The request MUST include a header field with the name |Origin| [RFC6454] if the request is coming from a browser client. 
            // If the connection is from a non-browser client, the request MAY include this header field if the semantics of that client match the use-case described here for browser clients.
            // More on Origin Considerations: http://tools.ietf.org/html/rfc6455#section-10.2
            if (!string.IsNullOrEmpty(origin))
                InternalRequest.SetHeader("Origin", origin);

            // The request MUST include a header field with the name |Sec-WebSocket-Version|.  The value of this header field MUST be 13.
            InternalRequest.SetHeader("Sec-WebSocket-Version", "13");

            if (!string.IsNullOrEmpty(protocol))
                InternalRequest.SetHeader("Sec-WebSocket-Protocol", protocol);

            // Disable caching
            InternalRequest.SetHeader("Cache-Control", "no-cache");
            InternalRequest.SetHeader("Pragma", "no-cache");
            
            InternalRequest.OnUpgraded = (req, resp) =>
                {
                    webSocket = resp as WebSocketResponse;

                    if (webSocket == null)
                    {
                        if (OnError != null)
                            OnError(this, req.Exception);

                        return;
                    }

                    if (OnOpen != null)
                        OnOpen(this);
                    
                    webSocket.OnText = (ws, msg) => {
                        if (OnMessage != null)
                            OnMessage(this, msg);
                    };

                    webSocket.OnBinary = (ws, bin) => {
                        if (OnBinary != null)
                            OnBinary(this, bin);
                    };

                    webSocket.OnClosed = (ws, code, msg) => {
                        if (OnClosed != null)
                            OnClosed(this, code, msg);
                    };

                    if (OnIncompleteFrame != null)
                        webSocket.OnIncompleteFrame = (ws, frame) => {
                            if (OnIncompleteFrame != null)
                                OnIncompleteFrame(this, frame);
                        };

                    if (StartPingThread)
                        webSocket.StartPinging(Math.Min(PingFrequency, 100));
                };
        }

        #endregion

        #region Public Interface

        public void Open()
        {
            if (requestSent || InternalRequest == null)
                return;

            InternalRequest.Send();
            requestSent = true;
        }

        public void Send(string message)
        {
            if (IsOpen)
                webSocket.Send(message);
        }

        public void Send(byte[] buffer)
        {
            if (IsOpen)
                webSocket.Send(buffer);
        }

        public void Send(IWebSocketFrameWriter frame)
        {
            if (IsOpen)
                webSocket.Send(frame);
        }

        public void Close()
        {
            if (IsOpen)
                webSocket.Close();
        }

        public void Close(UInt16 code, string message)
        {
            if (IsOpen)
                webSocket.Close(code, message);
        }

        #endregion

        #region Private Helpers

        private string GetSecKey(object[] from)
        {
            byte[] keys = new byte[16];
            int pos = 0;

            for (int i = 0; i < from.Length; ++i)
            {
                byte[] hash = BitConverter.GetBytes((Int32)from[i].GetHashCode());

                for (int cv = 0; cv < hash.Length && pos < keys.Length; ++cv)
                    keys[pos++] = hash[cv];
            }

            return Convert.ToBase64String(keys);
        }

        #endregion
    }
}