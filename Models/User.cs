using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEBCore.Models;

namespace WEBTest.Models
{
    //针对前端的model
    public class User
    {
        public User() { }
        public int Id { get; set; }
        public string account { get; set; }
        public string password { get; set; }

        public string address { get; set; }
        public string emial { get; set; }

        public List<Friend> Friends { get; set; }
    }
}
