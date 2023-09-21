namespace Services.DTO_s.Request
{
    public record WithdrawalRequestDto(string accountName, string accountNumber, string bankName, int amount, string walletId, string password){}
}
