using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WSCore;

namespace WSChatClient
{
    class Program
    {
        private static async Task ChatWithServer()
        {
            using (ClientWebSocket ws = new ClientWebSocket())
            {
                try
                {
                    //Uri serverUri = new Uri("wss://auc-powerpoint.officeapps.live.com/pods/WebSocketFrontDoorServiceHandler.ashx?waccluster=PSG2&usid=834c393d-4867-484b-a811-0cf936c44130&wdoverrides=DevicePixelRatio%3A1%2CRenderGIFSlideShow%3Atrue");
                    //Uri serverUri = new Uri("ws://localhost:1123/WSChat/WSHandler.ashx");
                    Uri serverUri = new Uri("ws://localhost:58027/WSChatProxy/WSProxyHandler.ashx");
                    await ws.ConnectAsync(serverUri, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                while (true)
                {
                    Console.Write("Input message ('exit' to exit): ");
                    string msg = Console.ReadLine();
                    if (msg == "exit")
                    {
                        break;
                    }
                    ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                    await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);

                    WebSocketHelper.Read(ws, (byte[] bytes) => { Console.WriteLine(Encoding.UTF8.GetString(bytes)); });
                    //ArraySegment<byte> bytesReceived = new ArraySegment<byte>(new byte[1024]);
                    //WebSocketReceiveResult result = await ws.ReceiveAsync(bytesReceived, CancellationToken.None);
                    //Console.WriteLine(Encoding.UTF8.GetString(bytesReceived.Array, 0, result.Count));
                    if (ws.State != WebSocketState.Open)
                    {
                        break;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Task t = ChatWithServer();
            t.Wait();
        }
    }
}
