using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Services.DTO_s.Request
{
    public class TransactionCreationRequestDto
    {
        public string TransactionType { get; set; }
        public string TransactionRef { get; set; }
        public string Status { get; set; }
        public decimal RemainingBalance { get; set; }
        public decimal Amount { get; set; }
    }
}
