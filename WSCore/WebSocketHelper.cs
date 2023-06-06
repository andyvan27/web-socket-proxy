using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WSCore
{
    public static class WebSocketHelper
    {
        public static async Task Read(WebSocket webSocket, Action<byte[]> callback)
        {
            int bufferSize = 1024;
            var buffer = new byte[bufferSize];
            var offset = 0;
            var free = buffer.Length;
            while (true)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, free), CancellationToken.None);
                offset += result.Count;
                free -= result.Count;
                if (result.EndOfMessage) break;
                if (free == 0)
                {   
                    var newSize = buffer.Length + bufferSize;                    
                    if (newSize > 1024 * 1024 * 1024) //1G
                    {
                        throw new Exception("Maximum reading web socket buffer size exceeded");
                    }
                    var newBuffer = new byte[newSize];
                    Array.Copy(buffer, 0, newBuffer, 0, offset);
                    buffer = newBuffer;
                    free = buffer.Length - offset;
                }
            }
            int resultSize = buffer.Length - free;
            var resultBytes = new byte[resultSize];
            Array.Copy(buffer, 0, resultBytes, 0, resultSize);
            callback(resultBytes);
        }
    }
}
