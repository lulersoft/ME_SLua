#if !NETFX_CORE
#define LOCK_ON_FILE
#endif

using System;
using System.Collections.Generic;
using System.IO;
#if (!UNITY_WP8 && !UNITY_WINRT && !UNITY_METRO) || UNITY_EDITOR
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
#endif
using System.Text;
using System.Threading;
using BestHTTP.Caching;

using TcpClient = SocketEx.TcpClient;
using BestHTTP.Extensions;
using BestHTTP.Authentication;
using Org.BouncyCastle.Crypto.Tls;
using BestHTTP.Cookies;

#if NETFX_CORE
using System.Threading.Tasks;
#endif

namespace BestHTTP
{
    /// <summary>
    /// Represents and manages a connection to a server.
    /// </summary>
    internal sealed class HTTPConnection : IDisposable
    {
        private enum RetryCauses
        {
            /// <summary>
            /// The request processed without any special case.
            /// </summary>
            None,

            /// <summary>
            /// If the server closed the connection while we sending a request we should reconnect and send the request again. But we will try it once.
            /// </summary>
            Reconnect,

            /// <summary>
            /// We need an another try with Authorization header set.
            /// </summary>
            Authenticate,

            /// <summary>
            /// The proxy needs authentication.
            /// </summary>
            ProxyAuthenticate,
        }

        #region Public Properties

        /// <summary>
        /// The address of the server that this connection is bound to.
        /// </summary>
        internal string ServerAddress { get; private set; }

        /// <summary>
        /// The state of this connection.
        /// </summary>
        internal HTTPConnectionStates State { get; private set; }

        /// <summary>
        /// It's true if this connection is available to process a HTTPRequest.
        /// </summary>
        internal bool IsFree { get { return State == HTTPConnectionStates.Free; } }

        /// <summary>
        /// If the State is HTTPConnectionStates.Processing, then it holds a HTTPRequest instance. Otherwise it's null.
        /// </summary>
        internal HTTPRequest CurrentRequest { get; private set; }

        internal bool IsRemovable { get { return (DateTime.UtcNow - LastProcessTime) > HTTPManager.MaxConnectionIdleTime; } }

        /// <summary>
        /// When we start to process the current request. It's set after the connection is estabilished.
        /// </summary>
        internal DateTime StartTime { get; private set; }

        #endregion

        #region Private Properties

        private TcpClient Client;
        private Stream Stream;
        private DateTime LastProcessTime;
        private HTTPProxy Proxy;
        private bool HasProxy { get { return Proxy != null; } }

        #endregion

        internal HTTPConnection(string serverAddress)
        {
            this.ServerAddress = serverAddress;
            this.State = HTTPConnectionStates.Initial;
            this.LastProcessTime = DateTime.UtcNow;
        }

        internal void Process(HTTPRequest request)
        {
            if (State == HTTPConnectionStates.Processing)
                throw new Exception("Connection already processing a request!");

            StartTime = DateTime.MaxValue;
            State = HTTPConnectionStates.Processing;

            CurrentRequest = request;
#if NETFX_CORE
            Windows.System.Threading.ThreadPool.RunAsync(ThreadFunc);
#else
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadFunc));
#endif
        }

        internal void Recycle()
        {
            State = HTTPConnectionStates.Free;
            CurrentRequest = null;
        }

        #region Request Processing Implementation

        private
#if NETFX_CORE
            async
#endif
            void ThreadFunc(object param)
        {
            bool alreadyReconnected = false;
            bool redirected = false;

            RetryCauses cause = RetryCauses.None;

#if LOCK_ON_FILE
            object uriLock = null;
#endif

#if UNITY_WEBPLAYER
            // Right now, no caching supported in the webplayer
            if (!CurrentRequest.DisableCache)
                CurrentRequest.DisableCache = true;
#endif

            try
            {
                if (!HasProxy && CurrentRequest.HasProxy)
                    Proxy = CurrentRequest.Proxy;

                // Lock only if we will use the cached entity.
#if LOCK_ON_FILE
                if (!CurrentRequest.DisableCache)
                    Monitor.Enter(uriLock = HTTPCacheFileLock.Acquire(CurrentRequest.CurrentUri));
#endif

                // Try load the full response from an already saved cache entity. If the response 
                if (TryLoadAllFromCache())
                    return;

                if (Client != null && !Client.IsConnected())
                    Close();

                do // of while (reconnect)
                {
                    if (cause == RetryCauses.Reconnect)
                    {
                        Close();
#if NETFX_CORE
                        await Task.Delay(100);
#else
                        Thread.Sleep(100);
#endif
                    }

                    cause = RetryCauses.None;

                    // Connect to the server
#if NETFX_CORE
                    await
#endif
                    Connect();

                    if (State == HTTPConnectionStates.AbortRequested)
                        throw new Exception("AbortRequested");

                    lock(HTTPManager.Locker)
                        StartTime = DateTime.UtcNow;

                    // Setup cache control headers before we send out the request
                    if (!CurrentRequest.DisableCache)
                        HTTPCacheService.SetHeaders(CurrentRequest);

                    // Write the request to the stream
                    // sentRequest will be true if the request sent out successfully(no SocketException), so we can try read the response
                    bool sentRequest = CurrentRequest.SendOutTo(Stream);

                    // sentRequest only true if there are no exceptions during CurrentRequest.SendOutTo.
                    if (!sentRequest)
                    {
                        Close();

                        // We will try again only once
                        if (!alreadyReconnected)
                        {
                            alreadyReconnected = true;
                            cause = RetryCauses.Reconnect;
                        }
                    }

                    // If sending out the request succeded, we will try read the response.
                    if (sentRequest)
                    {
                        bool received = Receive();

                        if (!received && !alreadyReconnected)
                        {
                            alreadyReconnected = true;
                            cause = RetryCauses.Reconnect;
                        }

                        if (CurrentRequest.Response != null)
                        {
                            switch (CurrentRequest.Response.StatusCode)
                            {
                                // Not authorized
                                // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.4.2
                                case 401:
                                    {
                                        string authHeader = CurrentRequest.Response.GetFirstHeaderValue("www-authenticate");
                                        if (!string.IsNullOrEmpty(authHeader))
                                        {
                                            var digest = DigestStore.GetOrCreate(CurrentRequest.CurrentUri);
                                            digest.ParseChallange(authHeader);

                                            if (CurrentRequest.Credentials != null && digest.IsUriProtected(CurrentRequest.CurrentUri) && (!CurrentRequest.HasHeader("Authorization") || digest.Stale))
                                                cause = RetryCauses.Authenticate;
                                        }

                                        goto default;
                                    }

                                // Proxy authentication required
                                // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.4.8
                                case 407:
                                    {
                                        if (CurrentRequest.HasProxy)
                                        {
                                            string authHeader = CurrentRequest.Response.GetFirstHeaderValue("proxy-authenticate");
                                            if (!string.IsNullOrEmpty(authHeader))
                                            {
                                                var digest = DigestStore.GetOrCreate(CurrentRequest.Proxy.Address);
                                                digest.ParseChallange(authHeader);

                                                if (CurrentRequest.Proxy.Credentials != null && digest.IsUriProtected(CurrentRequest.Proxy.Address) && (!CurrentRequest.HasHeader("Proxy-Authorization") || digest.Stale))
                                                    cause = RetryCauses.ProxyAuthenticate;
                                            }
                                        }

                                        goto default;
                                    }

                                // Redirected
                                case 301: // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.3.2
                                case 302: // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.3.3
                                case 307: // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.3.8
                                case 308: // http://tools.ietf.org/html/rfc7238
                                    {
                                        if (CurrentRequest.RedirectCount >= CurrentRequest.MaxRedirects)
                                            goto default;
                                        CurrentRequest.RedirectCount++;

                                        string location = CurrentRequest.Response.GetFirstHeaderValue("location");
                                        if (!string.IsNullOrEmpty(location))
                                        {
                                            // Remove the previously set Host header.
                                            CurrentRequest.RemoveHeader("Host");

                                            // Set the Referer header to the last Uri.
                                            CurrentRequest.SetHeader("Referer", CurrentRequest.CurrentUri.ToString());

                                            // Set the new Uri, the CurrentUri will return this while the IsRedirected property is true
                                            CurrentRequest.RedirectUri = GetRedirectUri(location);

                                            // Discard the redirect response, we don't need it any more
                                            CurrentRequest.Response = null;

                                            redirected = CurrentRequest.IsRedirected = true;
                                        }
                                        else
                                            #if !NETFX_CORE
                                                throw new MissingFieldException(string.Format("Got redirect status({0}) without 'location' header!", CurrentRequest.Response.StatusCode.ToString()));
                                            #else
                                                throw new Exception(string.Format("Got redirect status({0}) without 'location' header!", CurrentRequest.Response.StatusCode.ToString()));
                                            #endif

                                        goto default;
                                    }

                                default:
                                    if (CurrentRequest.IsCookiesEnabled)
                                        CookieJar.Set(CurrentRequest.Response);

                                    TryStoreInCache();
                                    break;
                            }

                            // If we have a response and the server telling us that it closed the connection after the message sent to us, then 
                            //  we will colse the connection too.
                            if (CurrentRequest.Response == null ||
                                CurrentRequest.Response.HasHeaderWithValue("connection", "close") || 
                                CurrentRequest.UseAlternateSSL)
                                Close();
                        }
                    }

                } while (cause != RetryCauses.None);
            }
            catch(TimeoutException e)
            {
                CurrentRequest.Response = null;
                CurrentRequest.Exception = e;
                CurrentRequest.State = HTTPRequestStates.ConnectionTimedOut;
            }
            catch (Exception e)
            {
                if (CurrentRequest.UseStreaming)
                    HTTPCacheService.DeleteEntity(CurrentRequest.CurrentUri);

                // Something gone bad, Response must be null!
                CurrentRequest.Response = null;

                switch(State)
                {
                    case HTTPConnectionStates.AbortRequested:
                        CurrentRequest.Exception = e;
                        CurrentRequest.State = HTTPRequestStates.Error;
                        break;
                    case HTTPConnectionStates.TimedOut:
                        CurrentRequest.State = HTTPRequestStates.TimedOut;
                        break;
                    default:
                        CurrentRequest.State = HTTPRequestStates.Aborted;
                        break;
                }

                Close();
            }
            finally
            {
#if LOCK_ON_FILE
                if (!CurrentRequest.DisableCache && uriLock != null)
                    Monitor.Exit(uriLock);
#endif

                // Avoid state changes. While we are in this block changing the connection's State, on Unity's main thread
                //  the HTTPManager's OnUpdate will check the connections's State and call functions that can change the inner state of
                //  the object. (Like setting the CurrentRequest to null in function Recycle() causing a NullRef exception)
                lock (HTTPManager.Locker)
                {
                    if (CurrentRequest != null && CurrentRequest.Response != null && CurrentRequest.Response.IsUpgraded)
                        State = HTTPConnectionStates.Upgraded;
                    else
                        State = redirected ? HTTPConnectionStates.Redirected : (Client == null ? HTTPConnectionStates.Closed : HTTPConnectionStates.WaitForRecycle);

                    // Change the request's state only when the whole processing finished
                    if (CurrentRequest.State == HTTPRequestStates.Processing && (State == HTTPConnectionStates.Closed || State == HTTPConnectionStates.WaitForRecycle))
                        CurrentRequest.State = HTTPRequestStates.Finished;

                    if (CurrentRequest.State == HTTPRequestStates.ConnectionTimedOut)
                        State = HTTPConnectionStates.Closed;

                    LastProcessTime = DateTime.UtcNow;
                }

                HTTPCacheService.SaveLibrary();
                CookieJar.Persist();
            }
        }

        private string ReadTo(Stream stream, byte blocker)
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

        private
#if NETFX_CORE
            async Task
#else
            void
#endif
            Connect()
        {
            Uri uri = CurrentRequest.HasProxy ? CurrentRequest.Proxy.Address : CurrentRequest.CurrentUri;

            if (Client == null)
                Client = new TcpClient();

            if (!Client.Connected)
            {
                Client.ConnectTimeout = CurrentRequest.ConnectTimeout;

//#if NETFX_CORE
                //await
//#endif
                Client.Connect(uri.Host, uri.Port);
            }

            if (Stream == null)
            {
                if (HasProxy && !Proxy.IsTransparent)
                {
                    Stream = Client.GetStream();

                    var outStream = new BinaryWriter(Stream);
                    outStream.Write(string.Format("CONNECT {0}:{1} HTTP/1.1", CurrentRequest.CurrentUri.Host, CurrentRequest.CurrentUri.Port).GetASCIIBytes());
                    outStream.Write(HTTPRequest.EOL);
                    outStream.Write(string.Format("Host: {0}:{1}", CurrentRequest.CurrentUri.Host, CurrentRequest.CurrentUri.Port).GetASCIIBytes());
                    outStream.Write(HTTPRequest.EOL);
                    outStream.Write(string.Format("Proxy-Connection: Keep-Alive"));
                    outStream.Write(HTTPRequest.EOL);
                    outStream.Write(HTTPRequest.EOL);
                    outStream.Flush();
                    
                    ReadTo(Stream, HTTPResponse.LF);
                    ReadTo(Stream, HTTPResponse.LF);
                }
                
                if (HTTPProtocolFactory.IsSecureProtocol(uri))
                {
                    // On WP8 there are no Mono, so we must use the 'alternate' TlsHandlers
#if !UNITY_WP8 && !NETFX_CORE
                    if (CurrentRequest.UseAlternateSSL)
                    {
#endif
                        var handler = new TlsClientProtocol(Client.GetStream(), new Org.BouncyCastle.Security.SecureRandom());
                        handler.Connect(new LegacyTlsClient(new AlwaysValidVerifyer()));
                        Stream = handler.Stream;
#if !UNITY_WP8 && !NETFX_CORE
                    }
                    else
                    {
                        SslStream sslStream = new SslStream(Client.GetStream(), false, (sender, cert, chain, errors) =>
                        {
                            return CurrentRequest.CallCustomCertificationValidator(cert, chain);
                        });

                        if (!sslStream.IsAuthenticated)
                            sslStream.AuthenticateAsClient(uri.Host);
                        Stream = sslStream;
                    }
#endif
                }
                else
                    Stream = Client.GetStream();
            }
        }

        private bool Receive()
        {
            CurrentRequest.Response = HTTPProtocolFactory.Get(HTTPProtocolFactory.GetProtocolFromUri(CurrentRequest.CurrentUri), CurrentRequest, Stream, CurrentRequest.UseStreaming, false);

            if (!CurrentRequest.Response.Receive())
            {
                CurrentRequest.Response = null;
                return false;
            }

            // We didn't check HTTPManager.IsCachingDisabled's value on purpose. (sending out a request with conditional get then change IsCachingDisabled to true may produce undefined behavior)
            if (CurrentRequest.Response.StatusCode == 304)
            {
                int bodyLength;
                using (var cacheStream = HTTPCacheService.GetBody(CurrentRequest.CurrentUri, out bodyLength))
                {
                    if (!CurrentRequest.Response.HasHeader("content-length"))
                        CurrentRequest.Response.Headers.Add("content-length", new List<string>(1) { bodyLength.ToString() });
                    CurrentRequest.Response.IsFromCache = true;
                    CurrentRequest.Response.ReadRaw(cacheStream, bodyLength);
                }
            }

            return true;
        }

        #endregion

        #region Helper Functions

        private bool TryLoadAllFromCache()
        {
            if (CurrentRequest.DisableCache)
                return false;

            // We will try read the response from the cache, but if something happens we will fallback to the normal way.
            try
            {
                //Unless specifically constrained by a cache-control (section 14.9) directive, a caching system MAY always store a successful response (see section 13.8) as a cache entity,
                //  MAY return it without validation if it is fresh, and MAY    return it after successful validation.
                // MAY return it without validation if it is fresh!
                if (HTTPCacheService.IsCachedEntityExpiresInTheFuture(CurrentRequest))
                {
                    CurrentRequest.Response = HTTPCacheService.GetFullResponse(CurrentRequest);

                    if (CurrentRequest.Response != null)
                        return true;
                }
            }
            catch
            {
                HTTPCacheService.DeleteEntity(CurrentRequest.CurrentUri);
            }

            return false;
        }

        private void TryStoreInCache()
        {
            // if UseStreaming && !DisableCache then we already wrote the response to the cache
            if (!CurrentRequest.UseStreaming &&
                !CurrentRequest.DisableCache &&
                CurrentRequest.Response != null &&
                HTTPCacheService.IsCacheble(CurrentRequest.CurrentUri, CurrentRequest.MethodType, CurrentRequest.Response))
            {
                HTTPCacheService.Store(CurrentRequest.CurrentUri, CurrentRequest.MethodType, CurrentRequest.Response);
            }
        }

        private Uri GetRedirectUri(string location)
        {
            Uri result = null;
            try
            {
                result = new Uri(location);
            }
#if !NETFX_CORE
            catch (UriFormatException)
#else
            catch
#endif
            {
                // Sometimes the server sends back only the path and query component of the new uri
                var uri = CurrentRequest.Uri;
                var builder = new UriBuilder(uri.Scheme, uri.Host, uri.Port, location);
                result = builder.Uri;
            }

            return result;
        }

        internal void HandleProgressCallback()
        {
            if (CurrentRequest.OnProgress != null && CurrentRequest.DownloadProgressChanged)
            {
                CurrentRequest.OnProgress(CurrentRequest, CurrentRequest.Downloaded, CurrentRequest.DownloadLength);
                CurrentRequest.DownloadProgressChanged = false;
            }
        }

        internal void HandleCallback()
        {
            HandleProgressCallback();

            if (State == HTTPConnectionStates.Upgraded)
            {
                if (CurrentRequest != null && CurrentRequest.Response != null && CurrentRequest.Response.IsUpgraded)
                    CurrentRequest.UpgradeCallback();
                State = HTTPConnectionStates.WaitForProtocolShutdown;
            }
            else
                CurrentRequest.CallCallback();
        }

        internal void Abort(HTTPConnectionStates newState)
        {
            State = newState;
            if (Stream != null)
                Stream.Dispose();
        }

        private void Close()
        {
            if (Client != null)
            {
                try
                {
                    Client.Close();
                }
                catch
                {

                }
                finally
                {
                    Stream = null;
                    Client = null;
                }
            }
        }

        public void Dispose()
        {
            Close();
        }

        #endregion

    }
}