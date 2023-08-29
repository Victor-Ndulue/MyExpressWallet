namespace Services.Helpers.TransactionHelpers
{
    public class VerifyResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public string reference { get; set; }
        public decimal amount { get; set; }
        public bool success { get; set; }
        public string authorization_code { get; set; }
    }
}
