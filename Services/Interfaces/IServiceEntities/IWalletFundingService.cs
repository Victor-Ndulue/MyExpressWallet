using Services.Helpers;

namespace Services.Interfaces.IServiceEntities
{
    public interface IWalletFundingService
    {
        Task<StandardResponse<string>> FundWalletAccount(int requestAmount, string userEmail, string walletId);
    }
}
