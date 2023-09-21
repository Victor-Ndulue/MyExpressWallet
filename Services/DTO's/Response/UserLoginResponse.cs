using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO_s.Response
{
    public class UserLoginResponse
    {
        public string Message { get; set; }
        public string Token { get; set; }

        public UserLoginResponse(string userName, string token)
        {
            Message = $"Welcome {userName}";
            Token = token ;
        }
    }
}
