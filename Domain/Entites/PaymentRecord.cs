using Domain.BaseEntity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entites
{
    public class PaymentRecord : BaseClass
    {
        public bool IsFinalized { get; set; }
        [ForeignKey(nameof(UserWallet))]
        public string UserWalletId { get; set; }
        public UserWallet UserWallet { get; set; }

        public string reference { get; set; }
        public string recipient { get; set; }
        [Column(TypeName ="money")]
        public decimal amount { get; set; }
        public string transfer_code { get; set; }
        public string currency { get; set; }
        public string status { get; set; }
        public string source { get; set; }
        public string reason { get; set; }
        public int PublicId { get; set; }
        public int integration { get; set; }
        public string domain { get; set; }
    }
}
