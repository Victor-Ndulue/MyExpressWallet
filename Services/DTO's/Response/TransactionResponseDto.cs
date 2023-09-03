namespace Services.DTO_s.Response
{
    public class TransactionResponseDto
    {
        public string TransactionId { get; set; }           
        public DateTime CreatedOn { get; set; }
        public string TransactionType { get; set; }
        public string TransactionRef { get; set; }
        public string Status { get; set; }
        public string RemainingBalance { get; set; }
        public string Amount { get; set; }
        public string SenderUserName { get; set; } 
        public string RecipientUserName { get; set; }
    }
}
