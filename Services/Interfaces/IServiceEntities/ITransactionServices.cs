using Domain.Entites;
using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers;
using Shared.PaginationDefiners;

namespace Services.Interfaces.IServiceEntities
{
    public interface ITransactionServices
    {
        Task<StandardResponse<TransactionResponseDto>> CreateTransaction(TransactionCreationRequestDto transactionCreationDto, string senderUserWalletId, string recipientUserWalletId);
        Task<StandardResponse<PagedList<TransactionResponseDto>>> GetAllTransactions(PaginationParams pagination);
        Task<StandardResponse<PagedList<TransactionResponseDto>>> GetAllUserTransactions(string userEmail, PaginationParams pagination);
        Task<StandardResponse<TransactionResponseDto>> GetTransactionByReferenceForUser(string transactionRef, string userEmail, string walletId);
        Task<StandardResponse<TransactionResponseDto>> UpdateTransaction(string transactionRef, decimal amount, decimal updatedBalance);
        Task<StandardResponse<IEnumerable<TransactionResponseDto>>> GetTransactionByDate(DateOnly date, string userEmail);
        Task<StandardResponse<string>> DeleteTransaction(string transactionRef, string userEmail);
    }
}
