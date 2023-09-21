namespace Services.Helpers.TransactionHelpers
{
    public class ResolveAccountDetailsPaystackResponseData
    {
        public string account_number { get; set; }
        public string account_name { get; set; }
        public int bank_id { get; set; }
        public string bank_code { get; set;}
    }
}
