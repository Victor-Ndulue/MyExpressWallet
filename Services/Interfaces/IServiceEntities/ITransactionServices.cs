using Services.DTO_s.Response;
using Services.Helpers;

namespace Services.Interfaces.IServiceEntities
{
    public interface ITransactionServices
    {
        IWalletFundingService WalletFundingService { get; }
        IVerifyPaymentService VerifyPaymentService { get; }

        Task<StandardResponse<TransactionResponseDto>> GetTransactionByReferenceForUser(string transactionRef, string userEmail);
        Task<StandardResponse<IEnumerable<TransactionResponseDto>>> GetAllTransactions(string userEmail);
        Task<StandardResponse<string>> DeleteTransaction(string transactionRef, string userEmail);
    }
}
