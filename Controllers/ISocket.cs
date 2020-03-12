using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WEBTest.Controllers
{
    public  interface ISocket
    {
       /// <summary>
       /// 异步的对接
       /// </summary>
       /// <param name="context"></param>
       /// <param name="webSocket"></param>
       /// <returns></returns>
       Task EchoAsync(HttpContext context, WebSocket webSocket);
    }
}
