namespace Services.DTO_s.Response
{
    public class InitializeWalletFundingResponseDto
    {
        public string WalletId { get; set; }
        public string TransactionReference {  get; set; }
        public string AuthorizationUrl { get; set; }
    }
}
