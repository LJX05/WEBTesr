using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WEBTest.Middleware
{
    public class RequesultIPMiddleware
    {
        private readonly RequestDelegate _next;
        
        public  RequesultIPMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke(HttpContext context) 
        {
            //等待方法之 前做些事
            Console.WriteLine("等待方法之 前做些事"+context.Connection.RemoteIpAddress.MapToIPv4());
            Console.WriteLine("前续1当前线程ID" + Thread.CurrentThread.ManagedThreadId);
            var a = context.Request.Body;
            await _next.Invoke(context);
            Console.WriteLine("后续1当前线程ID" + Thread.CurrentThread.ManagedThreadId);
        }
    }
}
