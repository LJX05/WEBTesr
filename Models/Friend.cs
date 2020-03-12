using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using WEBTest.Models;

namespace WEBCore.Models
{
    public class Friend
    {
        [Key]
        public int id { get; set; }
        //朋友id
        public int fid { get; set; }
        public User User { get; set; }
    }
}
