using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WEBTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Console.OutputEncoding = System.Text.Encoding.UTF8;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var a = CreateWebHostBuilder(args);
            var b = a.Build();//初始化启动类和依赖服务注入
            b.Run();
        }
        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            var a = Host.CreateDefaultBuilder(args);
            var b = a.ConfigureWebHostDefaults(
                webBuilder => webBuilder.UseStartup<Startup>()
                .UseKestrel().UseUrls("http://localhost:8029")
                );
            return b;
        }
    }
}
