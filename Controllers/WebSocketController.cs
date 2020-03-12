using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using WEBCore.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WEBTest.Controllers
{
    public class WebSocketController :ISocket
    {
        private IMemoryCache _cache;

        public WebSocketController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public async Task EchoAsync(HttpContext context, WebSocket webSocket)
        {
            var userID = context.Request.Cookies["userID"];
            var buffer = new byte[1024 * 4];
            var hashMap = (IDictionary<string, WebSocket>) _cache.Get("webSocketCache");
            hashMap.Add(userID, webSocket);
            Message msg = null;
            //接受离线的消息
            //从数据库中取出数据 发送出去
            //msg = new Message();
            //List<Message> messages = new List<Message>();
            //foreach (var it in messages) {
            //    var str=Newtonsoft.Json.JsonConvert.SerializeObject(it);
            //    buffer = System.Text.Encoding.Default.GetBytes(str);
            //    //await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
            //}

            //异步 IO等待接受消息 此代码底层会涉及IO操作 IO未接收到数据将会挂起等待
            //线程自动返回为空闲状态 既不会占用线程资源，注:也就是说此线程空闲后可以执行其他的任务。
            //当有数据时接收到时，操作系统会类似通知应用程序，应用程序将从线程池中分配线程任务继续执行后续操作
            //可在调试器堆栈信息看出此步叫做"异步恢复"
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                #region 群发消息 
                #endregion
                var data = System.Text.Encoding.Default.GetString(buffer,0,result.Count);
                try
                {
                    msg = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(data);
                    var ReceSocket = hashMap.FirstOrDefault(o=>o.Key== msg.ReceiveId.ToString()).Value;
                    if (ReceSocket != null)
                    {
                        msg.SenderId = int.Parse(userID);
                        await ReceSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                  //数据库持久化 离线消息 消息实体


                }
                catch (Exception ex){
                    Console.WriteLine(ex);
                }
                //异步 IO等待接受消息
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            //关闭连接后 移除对象
            hashMap.Remove(userID);
            webSocket.Dispose();
        }
    }
}