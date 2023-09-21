namespace Services.DTO_s.Request
{
    public record SendMoneyRequestDto(int amountToSend, string senderWalletId, string recipientWalletId, string loginPassword) {}
}
