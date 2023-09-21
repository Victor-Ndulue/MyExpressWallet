using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers;
using Services.Helpers.TransactionHelpers;

namespace Services.Interfaces.IServiceEntities
{
    public interface IWithdrawFromWalletService
    {
        Task<StandardResponse<TransactionResponseDto>> WithdrawFromWalletWithBankNameAndAccNumber(WithdrawalRequestDto withdrawalRequest, string userEmail, string bankCode);
        Task<StandardResponse<TransactionResponseDto>> WithdrawFromWallet(int amount, string walletId, string userEmail, string recipientAuth);
        Task<ResolveAccountDetailsPaystackResponse> ResolveBankAccount(string accountNumber, string bankName, string bankCode);
        Task<BankListResponse> GetBankList();
    }
}
