namespace Services.DTO_s.Request
{
    public class WalletCreationDto
    {
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "NGN";
    }
}
