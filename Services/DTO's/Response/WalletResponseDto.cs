namespace Services.DTO_s.Response
{
    public class WalletResponseDto
    {
        public string Id { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "NGN";
        public string UserName { get; set; }
    }
}
