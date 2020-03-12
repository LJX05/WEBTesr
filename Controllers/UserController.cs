
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
using WEBCore.Models;
using WEBTest.Models;

namespace WEBTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        static int flag = 0;
        private readonly DataContext _dbcontext;
        private readonly IUserService _userService;
        private IMemoryCache _cache;
        public UserController(IUserService userService, DataContext dbcontext ,IMemoryCache cache)
        {
            _cache = cache;
            _userService = userService;
            _dbcontext = dbcontext;
            
        }
        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }

        // GET api/values
        [HttpGet]
        public ActionResult<string[]> Get()
        {
            if (flag == 0)
            {
                HttpContext.Items["eee"] = 4546;
                flag = 1;
            }
            var a = HttpContext.Request.Body;
            Response.Cookies.Append("Uid", "111");

            return new string[] { a + "", "value2" };
        }
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            Console.WriteLine("+++++++");
            Thread.Sleep(10000);
            return "value" + id;
        }


        [Route("getFriends")]
        [HttpPost]
        public async Task<ActionResult<User>> getFriendsAsync([FromBody]int uid)
        {
            var user = await _dbcontext.Users.Include(o => o.Friends)
                            .SingleOrDefaultAsync(o => o.Id == uid);
            if (user != null)
            {
                return user;
            }
            return NotFound();
        }

        [HttpPost("Login")]
        public async Task<ActionResult<User>> LoginAsync([FromBody]User user)
        {
           var user1 = await _dbcontext.Users.Include(o => o.Friends).FirstOrDefaultAsync(o => o.account == user.account && o.password == user.password);
           if (user1 != null)
           {
                Response.Cookies.Append("UserID", user1.Id + "");
                return user1;
           }
            return NotFound();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        //注册
        [Route("SignIn")]
        [HttpPost]
        public async Task<ActionResult> SignInAsync([FromBody]User user)
        {

            await _dbcontext.Users.AddAsync(user);
            var staus = await _dbcontext.SaveChangesAsync();
            return Ok(staus);
        }

        [HttpGet("Socket")]
        public ActionResult<string> Socket()
        {
            //socketService(8013);
            socketServiceCallback(8013);
            return "ok";
        }

        #region 基于TAP模式
        private async void Client(Socket clientSocket, List<Socket> sockets)
        {
            //等待连接
            string result = string.Empty;
            byte[] recvBytes = new byte[1024];
            int bytes;
            //ArraySegment<byte> buffer, SocketFlags socketFlags
            var  obj=clientSocket.ReceiveAsync(new ArraySegment<byte>(recvBytes), SocketFlags.None);
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

        private async void socketService(int port)
        {
            IPAddress ip = IPAddress.Any;
            IPEndPoint ipe = new IPEndPoint(ip, port);
            var ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(ipe);
            ServerSocket.Listen(10);//开始监听
            var clientList = new List<Socket>();
            while (true)
            {
                //接受到client连接，为此连接建立新的socket，并接受信息
                //为新建连接创建新的socket(承诺这个异步会执行)
                Socket temp = await ServerSocket.AcceptAsync();
                clientList.Add(temp);
                Client(temp, clientList);
            }
        }
        #endregion

        private async void socketServiceCallback(int port)
        {
            IPAddress ip = IPAddress.Any;
            IPEndPoint ipe = new IPEndPoint(ip, port);
            var ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(ipe);
            ServerSocket.Listen(10);//开始监听
            var clientList = new List<Socket>();
            await Task.Run(() =>
            {
                while (true)
                {
                    //接受到client连接，为此连接建立新的socket，并接受信息
                    IAsyncResult asyncAccept=ServerSocket.BeginAccept((IAsyncResult asyncResult) =>
                    {
                        int i = clientList.Count;
                    //等待异步接收数据 线程阻塞
                    Socket clientSocket = ServerSocket.EndAccept(asyncResult);
                        clientList.Add(clientSocket);
                    //等待连接
                    string result = string.Empty;
                        byte[] recvBytes = new byte[1024];
                        int bytes;
                    //通过结束异步操作来阻止应用程序执行
                    IAsyncResult asynReceive = clientSocket.BeginReceive(recvBytes, 0, recvBytes.Length, SocketFlags.None, null, null);//从客户端接受信息
                                                                                                                                           //做一些其他事 。。。。
                    bytes = clientSocket.EndReceive(asynReceive);//会阻塞
                        result = Encoding.ASCII.GetString(recvBytes, 0, bytes);
                        while (result != "close")
                        {
                            //向客户端发送(回复)消息
                            IAsyncResult asynSend = clientSocket.BeginSend(recvBytes, 0, recvBytes.Length, SocketFlags.None, null, null);//从客户端接受信息
                                                                                                                                         //
                            asynSend.AsyncWaitHandle.WaitOne();
                            //接受发送                                                                                        //做一些其他事 。。。。
                            bytes = clientSocket.EndSend(asynSend);//会阻塞

                            //通过结束异步操作来阻止应用程序执行
                            asynReceive = clientSocket.BeginReceive(recvBytes, 0, recvBytes.Length, SocketFlags.None, null, null);//从客户端接受信息                                                                                                                 //做一些其他事 。。。。
                            bytes = clientSocket.EndReceive(asynReceive);//会阻塞
                            result = Encoding.ASCII.GetString(recvBytes, 0, bytes);
                        }
                        //移除列表里的对象
                        clientList.Remove(clientSocket);
                        clientSocket.Close();
                        clientSocket.Dispose();
                    }, null);
                    //因为是死循环所有要等待
                    asyncAccept.AsyncWaitHandle.WaitOne();
                }
            });
        }
    }
}
