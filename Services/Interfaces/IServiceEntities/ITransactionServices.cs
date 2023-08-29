namespace Services.Interfaces.IServiceEntities
{
    public interface ITransactionServices
    {
        Task<string> VerifyPayment(string reference, string userEmail, string walletId);
        Task<string> FundWalletAccount(int requestAmount, string userEmail, string walletId);
    }
}
