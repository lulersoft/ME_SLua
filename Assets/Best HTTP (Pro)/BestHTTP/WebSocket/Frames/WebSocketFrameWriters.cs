using System;
using System.IO;
using System.Text;

namespace BestHTTP.WebSocket.Frames
{
    /// <summary>
    /// Outgoing Frame's interface.
    /// The Frame Writers are helper classes to help send various kind of messages to the server.
    /// Each type should have its own writer.
    /// </summary>
    public interface IWebSocketFrameWriter
    {
        /// <summary>
        /// Type of the frame.
        /// </summary>
        WebSocketFrameTypes Type { get; }

        /// <summary>
        /// The final array of bytes that will be sent to the server.
        /// </summary>
        byte[] Get();
    }

    #region Data Frames

    /// <summary>
    /// Denotes a binary frame. The "Payload data" is arbitrary binary data whose interpretation is solely up to the application layer. 
    /// This is the base class of all other frame writers, as all frame can be represented as a byte array.
    /// </summary>
    public class WebSocketBinaryFrame : IWebSocketFrameWriter
    {
        public virtual WebSocketFrameTypes Type { get { return WebSocketFrameTypes.Binary; } }
        public bool IsFinal { get; protected set; }

        protected byte[] Data { get; set; }
        protected UInt64 Pos { get; set; }
        protected UInt64 Length { get; set; }

        public WebSocketBinaryFrame(byte[] data)
            : this(data, 0, data != null ? (UInt64)data.Length : 0, true)
        {
        }

        public WebSocketBinaryFrame(byte[] data, bool isFinal)
            : this(data, 0, data != null ? (UInt64)data.Length : 0, isFinal)
        {
        }

        public WebSocketBinaryFrame(byte[] data, UInt64 pos, UInt64 length, bool isFinal)
        {
            this.Data = data;
            this.Pos = pos;
            this.Length = length;
            this.IsFinal = isFinal;
        }

        public virtual byte[] Get()
        {
            if (Data == null)
                Data = new byte[0];

            using (var ms = new MemoryStream((int)Length + 9))
            {
                // For the complete documentation for this section see:
                // http://tools.ietf.org/html/rfc6455#section-5.2

                byte finalBit = (byte)(IsFinal ? 0x80 : 0x0);

                // First byte: Final Bit + OpCode
                ms.WriteByte((byte)(finalBit | (byte)Type));

                // The length of the "Payload data", in bytes: if 0-125, that is the payload length.  If 126, the following 2 bytes interpreted as a
                // 16-bit unsigned integer are the payload length.  If 127, the following 8 bytes interpreted as a 64-bit unsigned integer (the
                // most significant bit MUST be 0) are the payload length.  Multibyte length quantities are expressed in network byte order.
                if (Length < 126)
                    ms.WriteByte((byte)(0x80 | (byte)Length));
                else if (Length < UInt16.MaxValue)
                {
                    ms.WriteByte((byte)(0x80 | 126));
                    byte[] len = BitConverter.GetBytes((UInt16)Length);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(len, 0, len.Length);

                    ms.Write(len, 0, len.Length);
                }
                else
                {
                    ms.WriteByte((byte)(0x80 | 127));
                    byte[] len = BitConverter.GetBytes((UInt64)Length);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(len, 0, len.Length);

                    ms.Write(len, 0, len.Length);
                }

                // All frames sent from the client to the server are masked by a 32-bit value that is contained within the frame.  This field is
                // present if the mask bit is set to 1 and is absent if the mask bit is set to 0.
                // If the data is being sent by the client, the frame(s) MUST be masked.
                byte[] mask = BitConverter.GetBytes((Int32)this.GetHashCode());
                ms.Write(mask, 0, mask.Length);

                // Do the masking.
                for (UInt64 i = Pos; i < (Pos + Length); ++i)
                    ms.WriteByte((byte)(Data[i] ^ mask[(i - Pos) % 4]));

                return ms.ToArray();
            }
        }
    }

    /// <summary>
    /// The "Payload data" is text data encoded as UTF-8.  Note that a particular text frame might include a partial UTF-8 sequence; however, the whole message MUST contain valid UTF-8. 
    /// </summary>
    public sealed class WebSocketTextFrame : WebSocketBinaryFrame
    {
        public override WebSocketFrameTypes Type { get { return WebSocketFrameTypes.Text; } }

        public WebSocketTextFrame(string text)
            : base(Encoding.UTF8.GetBytes(text))
        {
        }
    }

    /// <summary>
    /// A fragmented message's first frame's contain the type of the message(binary or text), all consecutive frame of that message must be a Continuation frame.
    /// Last of these frame's Fin bit must be 1.
    /// </summary>
    /// <example>For a text message sent as three fragments, the first fragment would have an opcode of 0x1 (text) and a FIN bit clear, 
    /// the second fragment would have an opcode of 0x0 (Continuation) and a FIN bit clear, 
    /// and the third fragment would have an opcode of 0x0 (Continuation) and a FIN bit that is set.</example>
    public sealed class WebSocketContinuationFrame : WebSocketBinaryFrame
    {
        public override WebSocketFrameTypes Type { get { return WebSocketFrameTypes.Continuation; } }

        public WebSocketContinuationFrame(byte[] data, bool isFinal)
            : base(data, 0, (UInt64)data.Length, isFinal)
        {
        }

        public WebSocketContinuationFrame(byte[] data, UInt64 pos, UInt64 length, bool isFinal)
            : base(data, pos, length, isFinal)
        {
        }
    }

    #endregion

    #region Control Frames

    // All control frames MUST have a payload length of 125 bytes or less and MUST NOT be fragmented.

    /// <summary>
    /// The Ping frame contains an opcode of 0x9. A Ping frame MAY include "Application data".
    /// </summary>
    public sealed class WebSocketPing : WebSocketBinaryFrame
    {
        public override WebSocketFrameTypes Type { get { return WebSocketFrameTypes.Ping; } }

        public WebSocketPing(string msg)
            : base(Encoding.UTF8.GetBytes(msg))
        {
        }
    }

    /// <summary>
    /// A Pong frame sent in response to a Ping frame must have identical "Application data" as found in the message body of the Ping frame being replied to.
    /// </summary>
    public sealed class WebSocketPong : WebSocketBinaryFrame
    {
        public override WebSocketFrameTypes Type { get { return WebSocketFrameTypes.Pong; } }

        /// <summary>
        /// A Pong frame sent in response to a Ping frame must have identical "Application data" as found in the message body of the Ping frame being replied to.
        /// </summary>
        public WebSocketPong(WebSocketFrameReader ping)
            : base(ping.Data)
        {

        }
    }

    /// <summary>
    /// The Close frame MAY contain a body (the "Application data" portion of the frame) that indicates a reason for closing, 
    /// such as an endpoint shutting down, an endpoint having received a frame too large, or an endpoint having received a frame that
    /// does not conform to the format expected by the endpoint.
    /// As the data is not guaranteed to be human readable, clients MUST NOT show it to end users. 
    /// </summary>
    public sealed class WebSocketClose : WebSocketBinaryFrame
    {
        public override WebSocketFrameTypes Type { get { return WebSocketFrameTypes.ConnectionClose; } }

        public WebSocketClose()
            : base(null)
        {
        }

        public WebSocketClose(UInt16 code, string message)
            : base(WebSocketClose.GetData(code, message))
        {
        }

        private static byte[] GetData(UInt16 code, string message)
        {
            //If there is a body, the first two bytes of the body MUST be a 2-byte unsigned integer 
            // (in network byte order) representing a status code with value /code/ defined in Section 7.4 (http://tools.ietf.org/html/rfc6455#section-7.4). Following the 2-byte integer, 
            // the body MAY contain UTF-8-encoded data with value /reason/, the interpretation of which is not defined by this specification.
            // This data is not necessarily human readable but may be useful for debugging or passing information relevant to the script that opened the connection.
            int msgLen = Encoding.UTF8.GetByteCount(message);
            using (MemoryStream ms = new MemoryStream(2 + msgLen))
            {
                byte[] buff = BitConverter.GetBytes(code);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(buff, 0, buff.Length);

                ms.Write(buff, 0, buff.Length);

                buff = Encoding.UTF8.GetBytes(message);
                ms.Write(buff, 0, buff.Length);

                return ms.ToArray();
            }
        }
    }

    #endregion
}