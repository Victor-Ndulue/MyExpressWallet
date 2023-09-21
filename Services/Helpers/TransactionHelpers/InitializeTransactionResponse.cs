using Services.Helpers.TransactionHelpers;

namespace Services.Helpers.TransactionInitializeHelpers
{
    public class InitializeTransactionResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public InitializeTransactionDataResponse data { get; set; }
    }
}
