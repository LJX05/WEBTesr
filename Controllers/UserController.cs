
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ServiceDLL;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WEBTest.Models;

namespace WEBTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _dbcontext;
        private IMemoryCache _cache;

        private readonly IUserService _userService;
        public UserController(IUserService userService,IMemoryCache cache)
        {
               _cache = cache;
               _userService = userService;
        }
        static int flag = 0;
        // GET api/values
        [HttpGet]
        public  ActionResult<string[]> Get()
        {
            if (flag==0) {
                HttpContext.Items["eee"] =4546;
                flag = 1;
            }
             var a = HttpContext.Request.Body;
            Response.Cookies.Append("Uid", "111");

            return new string[] { a+"", "value2" };
        }
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            Console.WriteLine("+++++++");
            Thread.Sleep(10000);
            return "value" + id;
        }

        
        [HttpGet("Socket")]
        public  ActionResult<string> Socket()
        {
            socketService(8013);
            return "ok";
        }

        private async void socketService(int port) {
            IPAddress ip = IPAddress.Any;
            IPEndPoint ipe = new IPEndPoint(ip, port);
            var ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(ipe);
            ServerSocket.Listen(10);//开始监听
            var clientList= new List<Socket>();
            while (true) {
                //接受到client连接，为此连接建立新的socket，并接受信息
                //为新建连接创建新的socket(承诺这个异步会执行)
                Socket temp = await ServerSocket.AcceptAsync();
                clientList.Add(temp);
                Client(temp, clientList);
            } 
        }

        private async void Client(Socket clientSocket, List<Socket> sockets) {
            //等待连接
            string result = string.Empty;
            byte[] recvBytes = new byte[1024];
            int bytes;
            //ArraySegment<byte> buffer, SocketFlags socketFlags
            bytes = await clientSocket.ReceiveAsync(new ArraySegment<byte>(recvBytes), SocketFlags.None);//从客户端接受信息
            result = Encoding.ASCII.GetString(recvBytes, 0, bytes);
            //客户端不选择关闭则一直等待
            while (result != "close")
            {
                //向客户端发送(回复)消息
                await clientSocket.SendAsync(new ArraySegment<byte>(recvBytes), SocketFlags.None);
                int i = sockets.Count;
                bytes = await clientSocket.ReceiveAsync(new ArraySegment<byte>(recvBytes), SocketFlags.None);//从客户端接受信息
                result = Encoding.ASCII.GetString(recvBytes, 0, bytes);
            }
            //移除列表里的对象
            sockets.Remove(clientSocket);
            clientSocket.Close();
            clientSocket.Dispose();
        } 

        [HttpPost("Login")]
        public ActionResult<string> Login([FromBody]User user)
        {
            var a = HttpContext.Request.Body;
            _userService.Login(user.account,user.password);
            var b =a.Position = 0;
           var c = a.Length;
            HttpContext.Response.Cookies.Append("11","1111111");
            return a.ToString();
        }
        // POST api/values       

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }
    }
}
