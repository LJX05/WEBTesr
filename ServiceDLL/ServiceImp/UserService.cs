using ServiceDLL;
using System;

namespace ServiceDLL
{
    public class UserService : IUserService
    {
        

        public string Login(string account, string password)
        {
            if (account=="12345"&&password=="11111") {
                return "1";
            }
            return "0";
        }
    }
}
