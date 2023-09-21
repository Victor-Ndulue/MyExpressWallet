namespace Services.Helpers.TransactionHelpers
{
    public class TransferRecipientResponseData
    {
        public bool active { get; set; }
        public string currency { get; set; }
        public string recipient_code { get; set; }
        public string type { get; set; }    
    }
}
