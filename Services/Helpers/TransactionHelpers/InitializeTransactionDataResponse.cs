namespace Services.Helpers.TransactionHelpers
{
    public class InitializeTransactionDataResponse
    {
        public string authorization_url { get; set; }
        public string access_code { get; set; }
        public string reference { get; set; }
    }
}
