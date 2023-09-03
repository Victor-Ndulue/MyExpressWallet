using Domain.BaseEntity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entites
{
    public class Transaction : BaseClass
    {
        public string TransactionType { get; set; } = "fundwallet";
        public string TransactionRef { get; set; }
        public string Status { get; set; }
        [Column(TypeName = "money")]
        public decimal RemainingBalance { get; set; }
        [Column(TypeName = "money")]
        public decimal Amount { get; set; }
        public bool IsSenderDeleted { get; set; }
        public bool IsRecipientDeleted { get; set; }
        public UserWallet SenderUserWallet { get; set; }
        public UserWallet RecipientUserWallet { get; set; }
    }
}
