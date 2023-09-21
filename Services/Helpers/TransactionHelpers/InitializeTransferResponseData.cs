namespace Services.Helpers.TransactionHelpers
{
    public class InitializeTransferResponseData
    {
        public int Id { get; set; }
        public string reference { get; set; }
        public int recipient { get; set; }
        public int amount { get; set; }
        public string transfer_code { get; set; }
        public string status { get; set;}
        public string currency { get; set; }
        //public int integration { get; set; }
        //public string domain { get; set; }
        //public string source { get; set; }
        //public string reason { get; set; }
    }
}
