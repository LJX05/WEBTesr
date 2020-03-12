using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
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
    public class XML
    {
        public static IEnumerable<XElement> GetXElements() {
            XmlReader r = XmlReader.Create(@"D:\ProgramData\Microsoft\VisualStudio\source\repos\WEBTest\Middleware\XMLFile.xml");
            while (r.NodeType != XmlNodeType.Element)
                r.Read();
            XElement e = XElement.Load(r);
            return e.Elements();
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
            var type = Type.GetType(name);
            if (type == null)
            {
                throw new Exception("创建类型失败，因为未找到该类型"+name);
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
                    //打开连接
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var cache = _app.ApplicationServices.GetService<IMemoryCache>();
                    string name = context.Request.Path.Value.Split(_websocketPath + "/")[1];
                    var element=XML.GetXElements().FirstOrDefault(o=>o.Name=="bean"&& (o.Attribute("id")!=null&& o.Attribute("id").Value==name));
                    if (element == null)
                    {
                        context.Response.StatusCode = 400;
                    }
                    else {
                        var N = element.Attribute("class").Value;
                        //如果用一个工厂生产对象就好多了
                        ISocket webSocketController = ISocketFactory.CreatSocket(N, _app.ApplicationServices);
                        await webSocketController.EchoAsync(context, webSocket);
                    }
                    
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