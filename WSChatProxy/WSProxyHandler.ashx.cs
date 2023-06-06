using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;
using WSCore;

namespace WSChatProxy
{
    /// <summary>
    /// Summary description for WSHandler
    /// </summary>
    public class WSProxyHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
            {
                context.AcceptWebSocketRequest(HandleWebSocket);
            }
        }

        public bool IsReusable { get { return false; } }

        private async Task HandleWebSocket(AspNetWebSocketContext clientContext)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                var bridgeSocket = new ClientWebSocket();
                try
                {
                    await bridgeSocket.ConnectAsync(new Uri("ws://localhost:1123/WSChat/WSHandler.ashx"), cts.Token);
                }
                catch (Exception ex)
                {
                }

                while (true)
                {
                    try
                    {
                        ArraySegment<byte> clientBuffer = new ArraySegment<byte>(new byte[1024]);
                        await WebSocketHelper.Read(clientContext.WebSocket, (byte[] bytes) => { clientBuffer = new ArraySegment<byte>(bytes); });
                        if (clientContext.WebSocket.State == WebSocketState.Open && bridgeSocket.State == WebSocketState.Open)
                        {
                            try
                            {
                                await bridgeSocket.SendAsync(clientBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            catch (Exception ex)
                            {
                                break;
                            }
                            try
                            {
                                ArraySegment<byte> bridgeBuffer = new ArraySegment<byte>(new byte[1024]);
                                await WebSocketHelper.Read(bridgeSocket, (byte[] bytes) => { bridgeBuffer = new ArraySegment<byte>(bytes); });
                                try
                                {
                                    await clientContext.WebSocket.SendAsync(bridgeBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                                }
                                catch (Exception ex)
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }
            }
        }

        private async Task HandleWebSocketInParalell(AspNetWebSocketContext clientContext)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                var bridgeSocket = new ClientWebSocket();
                //bridgeSocket.Options.SetRequestHeader("Origin", new Uri(_proxyRequestUrl).GetLeftPart(UriPartial.Authority));
                //bridgeSocket.Options.SetRequestHeader("Origin", "https://auc-powerpoint.officeapps.live.com");
                //bridgeSocket.Options.SetRequestHeader("Origin", "https://powerpoint.officeapps.live.com");
                try
                {
                    //await bridgeSocket.ConnectAsync(new Uri($"wss:{_proxyRequestUrl.Substring("https:".Length)}"), cts.Token);
                    await bridgeSocket.ConnectAsync(new Uri("ws://localhost:1123/WSChat/WSHandler.ashx"), cts.Token);
                }
                catch (Exception ex)
                {
                    //SLog.Log(LoggingCategory.Proxy, Log.LogSeverity.Unexpected, "WebSocketHandler: HandleWebSocket: bridgeSocket.ConnectAsync: {0}", ex.ToString());
                }
                Task sending = new Task(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            ArraySegment<byte> clientBuffer = new ArraySegment<byte>(new byte[1024]);
                            var clientResult = await clientContext.WebSocket.ReceiveAsync(clientBuffer, CancellationToken.None);
                            if (clientContext.WebSocket.State == WebSocketState.Open && bridgeSocket.State == WebSocketState.Open)
                            {
                                try
                                {
                                    await bridgeSocket.SendAsync(clientBuffer, WebSocketMessageType.Text, clientResult.EndOfMessage, CancellationToken.None);
                                    if (clientResult.EndOfMessage)
                                        break;
                                }
                                catch (Exception ex)
                                {
                                    //SLog.Log(LoggingCategory.Proxy, Log.LogSeverity.Unexpected, "WebSocketHandler: HandleWebSocket: bridgeSocket.SendAsync: {0}", ex.ToString());
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            //SLog.Log(LoggingCategory.Proxy, Log.LogSeverity.Unexpected, "WebSocketHandler: HandleWebSocket: clientContext.WebSocket.ReceiveAsync: {0}", ex.ToString());
                            break;
                        }
                    }
                });
                Task receiving = new Task(async () =>
                {
                    while (true)
                    {
                        if (clientContext.WebSocket.State == WebSocketState.Open && bridgeSocket.State == WebSocketState.Open)
                        {
                            try
                            {
                                ArraySegment<byte> bridgeBuffer = new ArraySegment<byte>(new byte[1024]);
                                var bridgeResult = await bridgeSocket.ReceiveAsync(bridgeBuffer, CancellationToken.None);
                                try
                                {
                                    await clientContext.WebSocket.SendAsync(bridgeBuffer, WebSocketMessageType.Text, bridgeResult.EndOfMessage, CancellationToken.None);
                                    if (bridgeResult.EndOfMessage)
                                        break;
                                }
                                catch (Exception ex)
                                {
                                    //SLog.Log(LoggingCategory.Proxy, Log.LogSeverity.Unexpected, "WebSocketHandler: HandleWebSocket: clientContext.WebSocket.SendAsync: {0}", ex.ToString());
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                //SLog.Log(LoggingCategory.Proxy, Log.LogSeverity.Unexpected, "WebSocketHandler: HandleWebSocket: bridgeSocket.ReceiveAsync: {0}", ex.ToString());
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                });
                sending.Start();
                //sending.Wait();
                receiving.Start();
                //receiving.Wait();
                Task.WaitAll(sending, receiving);
            }
        }
    }
}