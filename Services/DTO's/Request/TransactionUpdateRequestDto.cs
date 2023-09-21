using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO_s.Request
{
    public class TransactionUpdateRequestDto
    {
        public decimal Amount { get; set; }
        public decimal RemainingBalance { get; set; }
    }
}
