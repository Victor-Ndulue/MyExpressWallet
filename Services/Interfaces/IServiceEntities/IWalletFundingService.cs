using Services.DTO_s.Response;
using Services.Helpers;

namespace Services.Interfaces.IServiceEntities
{
    public interface IWalletFundingService
    {
        Task<StandardResponse<InitializeWalletFundingResponseDto>> FundWalletAccount(int requestAmount, string userEmail, string walletId);
    }
}
