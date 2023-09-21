namespace Services.Helpers.TransactionHelpers
{
    public class VerifyTransactionResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
       public VerifyTransactionResponseData data { get; set; }
    }
}
