using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WEBTest.Controllers;

namespace WEBTest.Middleware
{
    //IApplicationBuilder app
    public static class webSocketMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<webSocketMiddleware>(builder);
        }
    }

    public class ISocketFactory
    {
        /// <summary>
        /// 生产方法
        /// </summary>
        /// <param name="name">类的名称</param>
        /// <param name="parmes">构造函数所传递的参数</param>
        /// <returns></returns>
        public static ISocket CreatSocket(string name, IServiceProvider provider)
        {
            var start = Environment.TickCount;
            var type = Type.GetType(name);
            if (type == null)
            {
                throw new Exception("is null");
            }
            //找到构造方法
            //var constructorM = type.GetMethods();
            var constructors = type.GetConstructors();
            var constr = constructors.FirstOrDefault(o => o.GetParameters().Length > 0);
            IList<object> parmes = new List<object>();
            if (constr != null)
            {
                var parameterInfos = constr.GetParameters();
                foreach (var item in parameterInfos)
                {
                    //找依赖注入项
                    var p = provider.GetService(item.ParameterType);
                    parmes.Add(p);
                }
            }
            //provider.GetService<T>();
            var socket=(ISocket)Activator.CreateInstance(type, parmes.ToArray());
            var end = Environment.TickCount; ;
            var a = end - start;
            return socket;
        }
    }

    public class webSocketMiddleware
    {
        private readonly IApplicationBuilder _app;
        private readonly RequestDelegate _next;
        private readonly string _websocketPath;

        public webSocketMiddleware(RequestDelegate next, IApplicationBuilder app, string path = "/ws")
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _websocketPath = path;
            _app = app;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(_websocketPath))
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var cache = _app.ApplicationServices.GetService<IMemoryCache>();
                    string name = context.Request.Path.Value.Split(_websocketPath + "/")[1];
                    //如果用一个工厂生产对象就好多了
                    ISocket webSocketController = ISocketFactory.CreatSocket("WEBTest.Controllers." + name + "Controller", _app.ApplicationServices);
                    await webSocketController.EchoAsync(context, webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        //内部类可以用 private 修饰 但是命名空间下的类 只能用public、internal
    }
}