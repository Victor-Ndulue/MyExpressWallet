using Services.Interfaces.IServiceEntities;

namespace Services.Interfaces.IServiceCommon
{
    public interface IServiceManager
    {
        IAuthenticationServices AuthenticationServices { get; }
        ITransactionServices TransactionServices { get; }
        IUserServices UserServices { get; }
        IVerifyWalletFundingService VerifyWalletFundingService { get; }
        IWalletFundingService WalletFundingService { get; }
        IWalletServices WalletServices { get; }
        IWithdrawFromWalletService WithdrawFromWalletService { get; }
    }
}
