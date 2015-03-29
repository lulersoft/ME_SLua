using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BestHTTP.WebSocket.Frames
{
    public enum WebSocketFrameTypes : byte
    {
        Continuation        = 0x0,
        Text                = 0x1,
        Binary              = 0x2,
        //Reserved1         = 0x3,
        //Reserved2         = 0x4,
        //Reserved3         = 0x5,
        //Reserved4         = 0x6,
        //Reserved5         = 0x7,
        ConnectionClose     = 0x8,
        Ping                = 0x9,
        Pong                = 0xA,
        //Reserved6         = 0xB,
        //Reserved7         = 0xC,
        //Reserved8         = 0xD,
        //Reserved9         = 0xE,
        //Reserved10        = 0xF,
    }
}