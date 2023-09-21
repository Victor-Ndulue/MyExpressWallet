using Services.DTO_s.Response;
using Services.Helpers;

namespace Services.Interfaces.IServiceEntities
{
    public interface IVerifyWalletFundingService
    {
        Task<StandardResponse<string>> VerifyPayment(string transactionReference, string userEmail, string walletId);
    }
}
