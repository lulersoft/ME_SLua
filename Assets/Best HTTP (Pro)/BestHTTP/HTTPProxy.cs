using System;
using BestHTTP.Authentication;

namespace BestHTTP
{
    public sealed class HTTPProxy
    {
        public Uri Address { get; set; }
        public Credentials Credentials { get; set; }

        public bool IsTransparent { get; set; }

        public HTTPProxy()
            :this(null, null, false)
        {}


        public HTTPProxy(Uri address)
            :this(address, null, false)
        {}

        public HTTPProxy(Uri address, Credentials credentials)
            :this(address, credentials, false)
        {}

        public HTTPProxy(Uri address, Credentials credentials, bool isTransparent)
        {
            this.Address = address;
            this.Credentials = credentials;
            this.IsTransparent = isTransparent;
        }
    }
}