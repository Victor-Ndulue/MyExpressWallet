namespace Services.Helpers.TransactionHelpers
{
    public class InitializeTransferResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public InitializeTransferResponseData data { get; set; }
    }
}
