using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceDLL;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using WEBTest.Middleware;
using WEBTest.Models;

namespace WEBTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
           
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {   
            //使用缓存服务
            services.AddMemoryCache();
            //域生命周期的UserService
            services.AddScoped<IUserService, UserService>();
            services.AddDistributedMemoryCache();
            services.AddSession();
            string connection = @"Server=.; Database=MyDemo;Trusted_Connection=True;MultipleActiveResultSets=true";
            services.AddDbContext<DataContext>(options => options.UseSqlServer(connection));


            services.AddControllers().AddNewtonsoftJson(
                options => options.SerializerSettings.ReferenceLoopHandling 
                = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region 初始化缓存
            var webCache = app.ApplicationServices.GetService<IMemoryCache>();
            var ff = app.ApplicationServices.GetService(typeof(IMemoryCache));
            IDictionary<string, WebSocket>  cacheWebSocket  =new Dictionary<string,WebSocket>();
            webCache.Set("webSocketCache", cacheWebSocket);
            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            //app.UseFileServer();
            app.UseHttpsRedirection();
            app.UseMiddleware<RequesultIPMiddleware>();
            app.UseSession();
            #region UseWebSocketsOptionsAO

            var webSocketOptions = new WebSocketOptions()

            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),

                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);

            #endregion UseWebSocketsOptionsAO
          
            app.UseWebSocketMiddleware();
            app.UseStaticFiles();
            app.UseRouting();

            //app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}