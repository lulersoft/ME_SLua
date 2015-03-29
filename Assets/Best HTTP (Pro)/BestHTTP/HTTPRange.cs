using System;

namespace BestHTTP
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class HTTPRange
    {
        /// <summary>
        /// The first byte's position that the server sent.
        /// </summary>
        public int FirstBytePos { get; private set; }

        /// <summary>
        /// The last byte's position that the server sent.
        /// </summary>
        public int LastBytePos { get; private set; }

        /// <summary>
        /// Indicates the total length of the full entity-body on the server, -1 if this length is unknown or difficult to determine.
        /// </summary>
        public int ContentLength { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsValid { get; private set; }

        internal HTTPRange()
        {
            this.ContentLength = -1;
            this.IsValid = false;
        }

        internal HTTPRange(int contentLength)
        {
            this.ContentLength = contentLength;
            this.IsValid = false;
        }

        internal HTTPRange(int fbp, int lbp, int contentLength)
        {
            this.FirstBytePos = fbp;
            this.LastBytePos = lbp;
            this.ContentLength = contentLength;

            // A byte-content-range-spec with a byte-range-resp-spec whose last-byte-pos value is less than its first-byte-pos value, or whose instance-length value is less than or equal to its last-byte-pos value, is invalid.
            this.IsValid = this.FirstBytePos <= this.LastBytePos && this.ContentLength > this.LastBytePos;
        }
    }
}