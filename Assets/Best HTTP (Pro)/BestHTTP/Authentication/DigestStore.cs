using System;
using System.Collections.Generic;
using System.Text;

namespace BestHTTP.Authentication
{
    /// <summary>
    /// Stores and manages already received digest infos.
    /// </summary>
    internal static class DigestStore
    {
        private static Dictionary<string, Digest> Digests = new Dictionary<string, Digest>();
        private static object Locker = new object();

        public static Digest Get(Uri uri)
        {
            lock (Locker)
            {
                Digest digest = null;
                if (Digests.TryGetValue(uri.Host, out digest))
                    if (!digest.IsUriProtected(uri))
                        return null;
                return digest;
            }
        }

        /// <summary>
        /// It will retrive or create a new Digest for the given Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Digest GetOrCreate(Uri uri)
        {
            lock (Locker)
            {
                Digest digest = null;
                if (!Digests.TryGetValue(uri.Host, out digest))
                    Digests.Add(uri.Host, digest = new Digest(uri));
                return digest;
            }
        }

        public static void Remove(Uri uri)
        {
            lock(Locker)
                Digests.Remove(uri.Host);
        }
    }
}