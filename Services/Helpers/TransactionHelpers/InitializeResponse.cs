namespace Services.Helpers.TransactionInitializeHelpers
{
    public class InitializeResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public string authorization_url { get; set; }
    }
}
