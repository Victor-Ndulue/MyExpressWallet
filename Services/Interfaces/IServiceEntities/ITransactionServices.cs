namespace Services.Interfaces.IServiceEntities
{
    public interface ITransactionServices
    {
        IWalletFundingService WalletFundingService { get; }
        IVerifyPaymentService VerifyPaymentService { get; }
    }
}
