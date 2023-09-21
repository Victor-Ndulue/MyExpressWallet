namespace Services.Helpers.TransactionHelpers
{
    public class BankListResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public ICollection<bankData> data {get; set;}
    }
}
