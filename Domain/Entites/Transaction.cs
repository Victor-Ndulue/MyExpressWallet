﻿using Domain.BaseEntity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entites
{
    public class Transaction : BaseClass
    {
        public string TransactionType { get; set; } 
        public string TransactionRef { get; set; }
        public string Status { get; set; }
        [Column(TypeName = "money")]
        public decimal RemainingBalance { get; set; }
        [Column(TypeName = "money")]
        public decimal Amount { get; set; }
        public bool IsSenderDeleted { get; set; }
        public bool IsRecipientDeleted { get; set; }

        [ForeignKey(nameof(UserWallet))]
        public string? SenderUserWalletId { get; set; }
        public UserWallet? SenderUserWallet { get; set; }

        [ForeignKey(nameof(UserWallet))]
        public string? RecipientUserWalletId { get; set; }
        public UserWallet? RecipientUserWallet { get; set; }   
    }
}
