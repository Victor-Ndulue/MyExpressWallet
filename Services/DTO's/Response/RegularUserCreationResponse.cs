using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO_s.Response
{
    public class RegularUserCreationResponse
    {
        public string Message { get; set; }
        public string Token { get; set; }
        public WalletResponseDto WalletResponse { get; set; }

        public RegularUserCreationResponse(string userName, string token, WalletResponseDto walletResponse)
        {
            Message = $"Welcome {userName}";
            Token = token;
            WalletResponse = walletResponse;

        }
    }
}
