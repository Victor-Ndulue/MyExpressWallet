using Services.Helpers.TransactionHelpers;

namespace Services.Helpers
{
    public class VerifyTransactionResponseData
    {
        public string channel { get; set; }
        public string gateway_response { get; set; }
        public string reference { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public bool success { get; set; }
        public PayStackAuthorizationResponse authorization{ get; set; }
    }
}
