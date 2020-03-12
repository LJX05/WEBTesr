using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEBCore.Models
{
    public class Message
    {
        //发送者ID
        public int SenderId { get; set; }
        //接受者ID
        public int ReceiveId { get; set; }
        //信息
        public string msgInfo { get; set; }
    }
}
