namespace Services.Helpers.TransactionHelpers
{
    public class TransferRecipientResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public TransferRecipientResponseData data { get; set; }  
    }
}
