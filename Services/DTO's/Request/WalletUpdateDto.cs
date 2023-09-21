using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO_s.Request
{
    public record WalletUpdateDto
    {
        public decimal Balance { get; set; } 
        public string? payStackAuth { get; set; }
    }
}
