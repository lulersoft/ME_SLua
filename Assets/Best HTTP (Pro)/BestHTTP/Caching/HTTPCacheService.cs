using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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

namespace BestHTTP.Caching
{
    using BestHTTP.Extensions;

    public static class HTTPCacheService
    {
        #region Private Properties And Fields

        /// <summary>
        /// Library file-format versioning support
        /// </summary>
        private const int LibraryVersion = 1;

        private static Dictionary<Uri, HTTPCacheFileInfo> library;
        private static Dictionary<Uri, HTTPCacheFileInfo> Library { get { LoadLibrary(); return library; } }

        internal static string CacheFolder { get; set; }
        private static string LibraryPath { get; set; }

        private static bool InClearThread;
        private static bool InMaintainenceThread;

        #endregion

        #region Name Conversion Functions

        private static string GetFileNameFromUri(Uri uri)
        {
            return Convert.ToBase64String(uri.ToString().GetASCIIBytes()).Replace('/', '-');
        }

        private static Uri GetUriFromFileName(string fileName)
        {
            byte[] buff = Convert.FromBase64String(fileName.Replace('-', '/'));
            string url = buff.AsciiToString();
            return new Uri(url);
        }

        #endregion

        #region Common Functions

        internal static void CheckSetup()
        {
#if !UNITY_WEBPLAYER
            try
            {
                SetupCacheFolder();
                LoadLibrary();
            }
            catch
            { }
#endif
        }

        internal static void SetupCacheFolder()
        {
#if !UNITY_WEBPLAYER
            try
            {
                if (string.IsNullOrEmpty(CacheFolder) || string.IsNullOrEmpty(LibraryPath))
                {
                    CacheFolder = System.IO.Path.Combine(HTTPManager.GetRootCacheFolder(), "HTTPCache");
                    if (!Directory.Exists(CacheFolder))
                        Directory.CreateDirectory(CacheFolder);

                    LibraryPath = System.IO.Path.Combine(HTTPManager.GetRootCacheFolder(), "Library");
                }
            }
            catch
            { }
#endif
        }

        internal static bool HasEntity(Uri uri)
        {
            lock (Library)
                return Library.ContainsKey(uri);
        }

        internal static bool DeleteEntity(Uri uri, bool removeFromLibrary = true)
        {
            object uriLocker = HTTPCacheFileLock.Acquire(uri);

            // To avoid a dead-lock we try acquire the lock on this uri only for a little time.
            // If we can't acquire it, its better to just return without risking a deadlock.
            if (Monitor.TryEnter(uriLocker, TimeSpan.FromSeconds(0.5f)))
            {
                try
                {
                    lock (Library)
                    {
                        HTTPCacheFileInfo info;
                        bool inStats = Library.TryGetValue(uri, out info);
                        if (inStats)
                            info.Delete();

                        if (inStats && removeFromLibrary)
                            Library.Remove(uri);

                        return true;
                    }
                }
                finally
                {
                    Monitor.Exit(uriLocker);
                }
            }

            return false;
        }

        internal static bool IsCachedEntityExpiresInTheFuture(HTTPRequest request)
        {
            HTTPCacheFileInfo info;
            lock (Library)
                if (Library.TryGetValue(request.CurrentUri, out info))
                    return info.WillExpireInTheFuture();

            return false;
        }

        /// <summary>
        /// Utility function to set the cache control headers according to the spec.: http://www.w3.org/Protocols/rfc2616/rfc2616-sec13.html#sec13.3.4
        /// </summary>
        /// <param name="request"></param>
        internal static void SetHeaders(HTTPRequest request)
        {
            HTTPCacheFileInfo info;
            lock (Library)
                if (Library.TryGetValue(request.CurrentUri, out info))
                    info.SetUpRevalidationHeaders(request);
        }

        #endregion

        #region Get Functions

        internal static System.IO.Stream GetBody(Uri uri, out int length)
        {
            length = 0;

            HTTPCacheFileInfo info;
            lock (Library)
                if (Library.TryGetValue(uri, out info))
                    return info.GetBodyStream(out length);

            return null;
        }

        internal static HTTPResponse GetFullResponse(HTTPRequest request)
        {
            HTTPCacheFileInfo info;
            lock (Library)
                if (Library.TryGetValue(request.CurrentUri, out info))
                    return info.ReadResponseTo(request);

            return null;
        }

        #endregion

        #region Storing

        /// <summary>
        /// Checks if the given response can be cached. http://www.w3.org/Protocols/rfc2616/rfc2616-sec13.html#sec13.4
        /// </summary>
        /// <returns>Returns true if cacheable, false otherwise.</returns>
        internal static bool IsCacheble(Uri uri, HTTPMethods method, HTTPResponse response)
        {
            if (method != HTTPMethods.Get)
                return false;

            if (response == null)
                return false;

            // Already cached
            if (response.StatusCode == 304)
                return false;

            if (response.StatusCode < 200 || response.StatusCode >= 400)
                return false;

            //http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.9.2
            var cacheControls = response.GetHeaderValues("cache-control");
            if (cacheControls != null && cacheControls[0].ToLower().Contains("no-store"))
                return false;

            var pragmas = response.GetHeaderValues("pragma");
            if (pragmas != null && pragmas[0].ToLower().Contains("no-cache"))
                return false;

            // Responses with byte ranges not supported yet.
            var byteRanges = response.GetHeaderValues("content-range");
            if (byteRanges != null)
                return false;

            return true;
        }

        internal static HTTPCacheFileInfo Store(Uri uri, HTTPMethods method, HTTPResponse response)
        {
            if (response == null || response.Data == null || response.Data.Length == 0)
                return null;

            HTTPCacheFileInfo info = null;

            lock (Library)
            {
                if (!Library.TryGetValue(uri, out info))
                    Library.Add(uri, info = new HTTPCacheFileInfo(uri));

                try
                {
                    info.Store(response);
                }
                catch
                {
                    // If something happens while we write out the response, than we will delete it becouse it might be in an invalid state.
                    DeleteEntity(uri);

                    throw;
                }
            }

            return info;
        }

        internal static System.IO.Stream PrepareStreamed(Uri uri, HTTPResponse response)
        {
            HTTPCacheFileInfo info;

            lock (Library)
            {
                if (!Library.TryGetValue(uri, out info))
                    Library.Add(uri, info = new HTTPCacheFileInfo(uri));

                try
                {
                    return info.GetSaveStream(response);
                }
                catch
                {
                    // If something happens while we write out the response, than we will delete it becouse it might be in an invalid state.
                    DeleteEntity(uri);

                    throw;
                }
            }
        }

        #endregion

        #region Public Maintanance Functions

        /// <summary>
        /// Deletes all cache entity. Non blocking.
        /// <remarks>Call it only if there no requests currently processed, becouse cache entries can be deleted while a server sends back a 304 result, so there will be no data to read from the cache!</remarks>
        /// </summary>
        public static void BeginClear()
        {
#if !UNITY_WEBPLAYER
            if (InClearThread)
                return;
            InClearThread = true;

            SetupCacheFolder();

#if !NETFX_CORE
            ThreadPool.QueueUserWorkItem(new WaitCallback((param) =>
#else
            Windows.System.Threading.ThreadPool.RunAsync((param) =>
#endif
            {
                    try
                    {
                        string[] cacheEntries = Directory.GetFiles(CacheFolder);

                        for (int i = 0; i < cacheEntries.Length; ++i)
                        {
                            // We need a try-catch block becouse between the Directory.GetFiles call and the File.Delete calls a maintainance job, or other file operations can delelete any file from the cache folder.
                            // So while there might be some problem with any file, we don't want to abort the whole for loop
                            try
                            {
                                string fileName = System.IO.Path.GetFileName(cacheEntries[i]);
                                DeleteEntity(GetUriFromFileName(fileName));
                            }
                            catch
                            {}
                        }
                    }
                    finally
                    {
                        SaveLibrary();
                        InClearThread = false;
                    }
                }
#if !NETFX_CORE
                )
#endif
                );
#endif
        }

        /// <summary>
        /// Deletes all expired cache entity.
        /// <remarks>Call it only if there no requests currently processed, becouse cache entries can be deleted while a server sends back a 304 result, so there will be no data to read from the cache!</remarks>
        /// </summary>
        public static void BeginMaintainence(HTTPCacheMaintananceParams maintananceParam)
        {
#if !UNITY_WEBPLAYER
            if (maintananceParam == null)
                throw new ArgumentNullException("maintananceParams == null");

            if (InMaintainenceThread)
                return;

            InMaintainenceThread = true;

            SetupCacheFolder();

#if !NETFX_CORE
            ThreadPool.QueueUserWorkItem(new WaitCallback((param) =>
#else
            Windows.System.Threading.ThreadPool.RunAsync((param) =>
#endif
                {
                    try
                    {
                        lock (Library)
                        {
                            // Delete cache entries older than the given time.
                            DateTime deleteOlderAccessed = DateTime.UtcNow - maintananceParam.DeleteOlder;
                            List<Uri> removedEntities = new List<Uri>();
                            foreach (var kvp in Library)
                                if (kvp.Value.LastAccess < deleteOlderAccessed)
                                {
                                    if (DeleteEntity(kvp.Key, false))
                                        removedEntities.Add(kvp.Key);
                                }

                            for (int i = 0; i < removedEntities.Count; ++i)
                                Library.Remove(removedEntities[i]);
                            removedEntities.Clear();

                            ulong cacheSize = GetCacheSize();

                            // This step will delete all entries starting with the oldest LastAccess property while the cache size greater then the MaxCacheSize in the given param.
                            if (cacheSize > maintananceParam.MaxCacheSize)
                            {
                                List<HTTPCacheFileInfo> fileInfos = new List<HTTPCacheFileInfo>(library.Count);

                                foreach(var kvp in library)
                                    fileInfos.Add(kvp.Value);

                                fileInfos.Sort();

                                int idx = 0;
                                while (cacheSize >= maintananceParam.MaxCacheSize && idx < fileInfos.Count)
                                {
                                    try
                                    {
                                        var fi = fileInfos[idx];
                                        ulong length = (ulong)fi.BodyLength;

                                        DeleteEntity(fi.Uri);

                                        cacheSize -= length;
                                    }
                                    catch
                                    {}
                                    finally
                                    {
                                        ++idx;
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        SaveLibrary();
                        InMaintainenceThread = false;
                    }
                }
#if !NETFX_CORE
                )
#endif
                );
#endif
        }

        public static int GetCacheEntityCount()
        {
#if !UNITY_WEBPLAYER
            CheckSetup();

            lock(Library)
                return Library.Count;
#else
            return 0;
#endif
        }

        public static ulong GetCacheSize()
        {
            CheckSetup();

            ulong size = 0;
#if !UNITY_WEBPLAYER
            lock (Library)
                foreach (var kvp in Library)
                    if (kvp.Value.BodyLength > 0)
                        size += (ulong)kvp.Value.BodyLength;
#endif
            return size;
        }

        #endregion

        #region Cache Library Management

        private static void LoadLibrary()
        {
            // Already loaded?
            if (library != null)
                return;

            library = new Dictionary<Uri, HTTPCacheFileInfo>();

#if !UNITY_WEBPLAYER
            if (!File.Exists(LibraryPath))
            {
                DeleteUnusedFiles();
                return;
            }

            try
            {
                lock (library)
                {
                    using (var fs = new FileStream(LibraryPath, FileMode.Open))
                    using (var br = new System.IO.BinaryReader(fs))
                    {
                        int version = br.ReadInt32();
                        int statCount = br.ReadInt32();

                        for (int i = 0; i < statCount; ++i)
                        {
                            Uri uri = new Uri(br.ReadString());
                            bool onFileSystem = File.Exists(System.IO.Path.Combine(CacheFolder, GetFileNameFromUri(uri)));

                            if (onFileSystem)
                                library.Add(uri, new HTTPCacheFileInfo(uri, br, version));
                        }
                    }
                }

                DeleteUnusedFiles();
            }
            catch
            {}
#endif
        }

        internal static void SaveLibrary()
        {
#if !UNITY_WEBPLAYER
            if (library == null)
                return;

            try
            {
                lock (Library)
                {
                    using (var fs = new FileStream(LibraryPath, FileMode.Create))
                    using (var bw = new System.IO.BinaryWriter(fs))
                    {
                        bw.Write(LibraryVersion);
                        bw.Write(Library.Count);
                        foreach (var kvp in Library)
                        {
                            bw.Write(kvp.Key.ToString());

                            kvp.Value.SaveTo(bw);
                        }
                    }
                }
            }
            catch
            {}
#endif
        }


        internal static void SetBodyLength(Uri uri, int bodyLength)
        {
            lock (Library)
            {
                HTTPCacheFileInfo fileInfo;
                if (Library.TryGetValue(uri, out fileInfo))
                    fileInfo.BodyLength = bodyLength;
                else
                    Library.Add(uri, fileInfo = new HTTPCacheFileInfo(uri, DateTime.UtcNow, bodyLength));
            }
        }

        /// <summary>
        /// Deletes all files from the cache folder that isn't in the Library.
        /// </summary>
        private static void DeleteUnusedFiles()
        {
#if !UNITY_WEBPLAYER
            CheckSetup();

            string[] cacheEntries = Directory.GetFiles(CacheFolder);

            for (int i = 0; i < cacheEntries.Length; ++i)
            {
                // We need a try-catch block becouse between the Directory.GetFiles call and the File.Delete calls a maintainance job, or other file operations can delelete any file from the cache folder.
                // So while there might be some problem with any file, we don't want to abort the whole for loop
                try
                {
                    string fileName = System.IO.Path.GetFileName(cacheEntries[i]);
                    Uri uri = GetUriFromFileName(fileName);

                    lock (Library)
                        if (!Library.ContainsKey(uri))
                            DeleteEntity(uri);
                }
                catch
                {}
            }
#endif
        }

        #endregion
    }
}