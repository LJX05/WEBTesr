using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceDLL
{ 
     public interface IUserService
    {
        string Login(string account, string password);
    }
}
