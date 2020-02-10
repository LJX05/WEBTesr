using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WEB.Controllers
{
    public class WebSocketController
    {
        private IMemoryCache _cache;
        public WebSocketController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }
        public async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var list = (IList<WebSocket>)_cache.Get("webSocketCache");
            list.Add(webSocket);
            //异步 IO等待接受消息
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                #region 群发消息 
                foreach (var ItemSocket in list)
                {
                    await ItemSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                }
                #endregion
                //异步 IO等待接受消息
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            //关闭连接后 移除对象
            list.Remove(webSocket);
        }
    }
}