# web-socket-proxy
 Web Socket Proxy sample in C#

## Open in Visual Studio 2022
- Main solution file `WSChat\WSChat.sln`.
- Project `WSChat\WSChat.csproj` is a web site that has web socket server as a ASP.Net http handler and web socket client in javascript on a page.
- Project `WSChat\WSChatClient\WSChatClient.csproj` is a .Net web socket client console app
- Project `WSChatProxy\WSChatProxy.csproj` is another ASP.Net http handler acting as both web socket server and client to stay in the middle of WSChat and WSChatClient and forward back and forth web socket contents.

## References
- WSChat server and client are from this article: https://www.codeproject.com/Articles/618032/Using-WebSocket-in-NET-4-5-Part-2
- Other refs: 
    - https://www.codetinkerer.com/2018/06/05/aspnet-core-websockets.html
    - https://itq.eu/net-4-5-websocket-client-without-a-browser/


