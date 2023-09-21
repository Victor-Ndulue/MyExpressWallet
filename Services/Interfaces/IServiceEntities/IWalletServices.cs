using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers;
using Shared.PaginationDefiners;

namespace Services.Interfaces.IServiceEntities
{
    public interface IWalletServices
    {
        Task<StandardResponse<WalletResponseDto>> CreateWalletAccount(string userId);
        Task<StandardResponse<TransactionResponseDto>> SendMoneyAsync(SendMoneyRequestDto requestDetails, string senderEmail);
        Task<StandardResponse<WalletResponseDto>> GetUserWalletById(string walletId);
        Task UpdateWallet(string walletId, decimal updatedBalance, string payStackAuth);
        Task<StandardResponse<string>> DeleteUserWallet(string walletId);
        Task<StandardResponse<PagedList<WalletResponseDto>>> GetAllWallets(PaginationParams pagination);
        Task<StandardResponse<PagedList<WalletResponseDto>>> GetAllUserWallets(string email, PaginationParams pagination);
        Task<StandardResponse<WalletResponseDto>> GetWalletById(string walletId);

        Task<StandardResponse<IEnumerable<WalletResponseDto>>> GetAllUserWalletsForWork(string email);

    }
}
