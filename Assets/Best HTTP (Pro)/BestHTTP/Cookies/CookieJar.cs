using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if NETFX_CORE
using FileStream = BestHTTP.PlatformSupport.IO.FileStream;
using Directory = BestHTTP.PlatformSupport.IO.Directory;
using File = BestHTTP.PlatformSupport.IO.File;

using BestHTTP.PlatformSupport.IO;
#else
using FileStream = System.IO.FileStream;
using Directory = System.IO.Directory;

using System.IO;
#endif

namespace BestHTTP.Cookies
{
    /// <summary>
    /// The Cookie Jar implementation based on RFC 6265(http://tools.ietf.org/html/rfc6265).
    /// </summary>
    public static class CookieJar
    {
        // Version of the cookie store. It may be used in a future version for maintaining compatibility.
        private const int Version = 1;

        #region Privates

        private static List<Cookie> Cookies = new List<Cookie>();
        private static string CookieFolder { get; set; }
        private static string LibraryPath { get; set; }
        private static object Locker = new object();

        #endregion

        #region Internal Functions

        internal static void SetupFolder()
        {
            try
            {
                if (string.IsNullOrEmpty(CookieFolder) || string.IsNullOrEmpty(LibraryPath))
                {
                    CookieFolder = System.IO.Path.Combine(HTTPManager.GetRootCacheFolder(), "Cookies");
                    LibraryPath = System.IO.Path.Combine(CookieFolder, "Library");
                }
            }
            catch
            { }
        }

        internal static void Set(HTTPResponse response)
        {
            if (response == null)
                return;

            lock(Locker)
            {
                try
                {
                    Maintain();

                    List<Cookie> newCookies = new List<Cookie>();
                    var setCookieHeaders = response.GetHeaderValues("set-cookie");

                    // No cookies. :'(
                    if (setCookieHeaders == null)
                        return;

                    foreach (var cookieHeader in setCookieHeaders)
                    {
                        try
                        {
                            Cookie cookie = Cookie.Parse(cookieHeader, response.baseRequest.CurrentUri);

                            if (cookie != null)
                            {
                                int idx;
                                var old = Find(cookie, out idx);

                                // if no value for the cookie or already expired then the server asked us to delete the cookie
                                bool expired = string.IsNullOrEmpty(cookie.Value) || !cookie.WillExpireInTheFuture();

                                if (!expired)
                                {
                                    // no old cookie, add it straith to the list
                                    if (old == null)
                                    {
                                        Cookies.Add(cookie);

                                        newCookies.Add(cookie);
                                    }
                                    else
                                    {
                                        // Update the creation-time of the newly created cookie to match the creation-time of the old-cookie.
                                        cookie.Date = old.Date;
                                        Cookies[idx] = cookie;

                                        newCookies.Add(cookie);
                                    }
                                }
                                else if (idx != -1) // delete the cookie
                                    Cookies.RemoveAt(idx);
                            }
                        }
                        catch
                        {
                            // Ignore cookie on error
                        }
                    }

                    response.Cookies = newCookies;
                }
                catch
                {}
            }
        }

        /// <summary>
        /// Deletes all expired or 'old' cookies, and will keep the sum size of cookies under the given size.
        /// </summary>
        internal static void Maintain()
        {
            // It's not the same as in the rfc:
            //  http://tools.ietf.org/html/rfc6265#section-5.3

            lock (Locker)
            {
                try
                {
                    uint size = 0;
                    TimeSpan accessThreshold = TimeSpan.FromDays(7);

                    for (int i = 0; i < Cookies.Count; )
                    {
                        var cookie = Cookies[i];

                        // Remove expired or not used cookies
                        if (!cookie.WillExpireInTheFuture() || (cookie.LastAccess + accessThreshold) < DateTime.UtcNow)
                            Cookies.RemoveAt(i);
                        else
                        {
                            if (!cookie.IsSession)
                                size += cookie.GuessSize();
                            i++;
                        }
                    }

                    if (size > HTTPManager.CookieJarSize)
                    {
                        Cookies.Sort();

                        while (size > HTTPManager.CookieJarSize && Cookies.Count > 0)
                        {
                            var cookie = Cookies[0];
                            Cookies.RemoveAt(0);

                            size -= cookie.GuessSize();
                        }
                    }
                }
                catch
                { }
            }
        }

        /// <summary>
        /// Saves the Cookie Jar to a file.
        /// </summary>
        /// <remarks>Not implemented under Unity WebPlayer</remarks>
        internal static void Persist()
        {
#if !UNITY_WEBPLAYER
            lock (Locker)
            {
                try
                {
                    // Delete any expired cookie
                    Maintain();

                    if (!Directory.Exists(CookieFolder))
                        Directory.CreateDirectory(CookieFolder);

                    using (var fs = new FileStream(LibraryPath, FileMode.Create))
                    using (var bw = new System.IO.BinaryWriter(fs))
                    {
                        bw.Write(Version);

                        // Count how many non-session cookies we have
                        int count = 0;
                        foreach (var cookie in Cookies)
                            if (!cookie.IsSession)
                                count++;

                        bw.Write(count);

                        // Save only the persistable cookies
                        foreach (var cookie in Cookies)
                            if (!cookie.IsSession)
                                cookie.SaveTo(bw);
                    }
                }
                catch
                { }
            }
#endif
        }

        /// <summary>
        /// Load previously persisted cooki library from the file.
        /// </summary>
        internal static void Load()
        {
#if !UNITY_WEBPLAYER
            lock (Locker)
            {
                try
                {
                    Cookies.Clear();

                    if (!Directory.Exists(CookieFolder))
                        Directory.CreateDirectory(CookieFolder);

                    if (!File.Exists(LibraryPath))
                        return;

                    using (var fs = new FileStream(LibraryPath, FileMode.Open))
                    using (var br = new System.IO.BinaryReader(fs))
                    {
                        /*int version = */br.ReadInt32();
                        int cookieCount = br.ReadInt32();

                        for (int i = 0; i < cookieCount; ++i)
                        {
                            Cookie cookie = new Cookie();
                            cookie.LoadFrom(br);

                            if (cookie.WillExpireInTheFuture())
                                Cookies.Add(cookie);
                        }
                    }
                }
                catch
                {
                    Cookies.Clear();
                }
            }
#endif
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Returns all Cookies that corresponds to the given Uri.
        /// </summary>
        public static List<Cookie> Get(Uri uri)
        {
            lock (Locker)
            {
                List<Cookie> result = new List<Cookie>();

                for (int i = 0; i < Cookies.Count; ++i)
                {
                    Cookie cookie = Cookies[i];
                    if (cookie.WillExpireInTheFuture() && uri.Host.IndexOf(cookie.Domain) != -1 && uri.AbsolutePath.StartsWith(cookie.Path))
                        result.Add(cookie);
                }

                return result;
            }
        }

        public static List<Cookie> GetAll()
        {
            return Cookies;
        }

        /// <summary>
        /// Deletes all cookies from the Jar.
        /// </summary>
        public static void Clear()
        {
            lock (Locker)
                Cookies.Clear();
        }

        /// <summary>
        /// Removes cookies that older than the given parameter.
        /// </summary>
        public static void Clear(TimeSpan olderThan)
        {
            lock (Locker)
            {
                for (int i = 0; i < Cookies.Count; )
                {
                    var cookie = Cookies[i];

                    // Remove expired or not used cookies
                    if (!cookie.WillExpireInTheFuture() || (cookie.Date + olderThan) < DateTime.UtcNow)
                        Cookies.RemoveAt(i);
                    else
                        i++;
                }
            }
        }

        /// <summary>
        /// Removes cookies that matches to the given domain.
        /// </summary>
        public static void Clear(string domain)
        {
            lock (Locker)
            {
                for (int i = 0; i < Cookies.Count; )
                {
                    var cookie = Cookies[i];

                    // Remove expired or not used cookies
                    if (!cookie.WillExpireInTheFuture() || cookie.Domain.IndexOf(domain) != -1)
                        Cookies.RemoveAt(i);
                    else
                        i++;
                }
            }
        }

        public static void Remove(Uri uri, string name)
        {
            lock(Locker)
            {
                for (int i = 0; i < Cookies.Count; )
                {
                    var cookie = Cookies[i];

                    if (cookie.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && uri.Host.IndexOf(cookie.Domain) != -1)
                        Cookies.RemoveAt(i);
                    else
                        i++;
                }
            }
        }

        #endregion

        #region Private Helper Functions

        /// <summary>
        /// Find and return a Cookie and his index in the list.
        /// </summary>
        private static Cookie Find(Cookie cookie, out int idx)
        {
            for (int i = 0; i < Cookies.Count; ++i)
            {
                Cookie c = Cookies[i];

                if (c.Equals(cookie))
                {
                    idx = i;
                    return c;
                }
            }

            idx = -1;
            return null;
        }

        #endregion
    }
}