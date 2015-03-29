using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BestHTTP.Extensions;

namespace BestHTTP.WebSocket.Frames
{
    /// <summary>
    /// Represents an incoming WebSocket Frame.
    /// </summary>
    public sealed class WebSocketFrameReader
    {
        #region Properties

        /// <summary>
        /// True if it's a final Frame in a sequence, or the only one.
        /// </summary>
        public bool IsFinal { get; private set; }

        /// <summary>
        /// The type of the Frame.
        /// </summary>
        public WebSocketFrameTypes Type { get; private set; }

        /// <summary>
        /// Indicates if there are any mask sent to decode the data.
        /// </summary>
        public bool HasMask { get; private set; }

        /// <summary>
        /// The length of the Data.
        /// </summary>
        public UInt64 Length { get; private set; }

        /// <summary>
        /// The sent byte array as a mask to decode the data.
        /// </summary>
        public byte[] Mask { get; private set; }

        /// <summary>
        /// The decoded array of bytes.
        /// </summary>
        public byte[] Data { get; private set; }

        #endregion

        internal void Read(Stream stream)
        {
            // For the complete documentation for this section see:
            // http://tools.ietf.org/html/rfc6455#section-5.2

            byte header = (byte)stream.ReadByte();

            // The first byte is the Final Bit and the type of the frame
            IsFinal = (header & 0x80) != 0;
            Type = (WebSocketFrameTypes)(header & 0xF);

            header = (byte)stream.ReadByte();

            // The secound byte is the Mask Bit and the length of the payload data
            HasMask = (header & 0x80) != 0;

            // if 0-125, that is the payload length.
            Length = (UInt64)(header & 127);

            // If 126, the following 2 bytes interpreted as a 16-bit unsigned integer are the payload length.
            if (Length == 126)
            {
                byte[] rawLen = new byte[2];
                stream.ReadBuffer(rawLen);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(rawLen, 0, rawLen.Length);

                Length = (UInt64)BitConverter.ToUInt16(rawLen, 0);
            }
            else if (Length == 127)
            {
                // If 127, the following 8 bytes interpreted as a 64-bit unsigned integer (the
                // most significant bit MUST be 0) are the payload length.

                byte[] rawLen = new byte[8];
                stream.ReadBuffer(rawLen);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(rawLen, 0, rawLen.Length);

                Length = (UInt64)BitConverter.ToUInt64(rawLen, 0);
            }

            // Read the Mask, if has any
            if (HasMask)
            {
                Mask = new byte[4];
                stream.Read(Mask, 0, 4);
            }

            Data = new byte[Length];
            for (UInt64 i = 0; i < Length; ++i)
            {
                Data[i] = (byte)stream.ReadByte();
                if (HasMask)
                    Data[i] = (byte)(Data[i] ^ Mask[i % 4]);
            }
        }

        /// <summary>
        /// Assembles all fragments into a final frame. Call this on the last fragment of a frame.
        /// </summary>
        /// <param name="fragments">The list of previously downloaded and parsed fragments of the frame</param>
        internal void Assemble(List<WebSocketFrameReader> fragments)
        {
            // this way the following algorithms will handle this fragment's data too
            fragments.Add(this);

            UInt64 finalLength = 0;
            for (int i = 0; i < fragments.Count; ++i)
                finalLength += fragments[i].Length;

            byte[] buffer = new byte[finalLength];
            UInt64 pos = 0;
            for (int i = 0; i < fragments.Count; ++i)
            {
                Array.Copy(fragments[i].Data, 0, buffer, (int)pos, (int)fragments[i].Length);
                pos += fragments[i].Length;
            }

            // All fragments of a message are of the same type, as set by the first fragment's opcode.
            this.Type = fragments[0].Type;
            this.Length = finalLength;
            this.Data = buffer;
        }
    }
}