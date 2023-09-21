namespace Services.Helpers.TransactionHelpers
{
    public class ResolveAccountDetailsPaystackResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public ResolveAccountDetailsPaystackResponseData data { get; set; }  
    }
}
