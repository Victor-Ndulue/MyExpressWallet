using Domain.BaseEntity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entites
{
    public class UserWallet : BaseClass
    {
        public string PayStackAuth { get; set; }
        [Column(TypeName = "money")]
        public decimal Balance { get; set; } 
        public string Currency { get; set; } = "NGN";
        public ICollection<Transaction> SentTransactions { get; set; }
        public ICollection<Transaction> ReceivedTransactions { get; set; }
        public AppUser AppUser { get; set; }
    }
}
