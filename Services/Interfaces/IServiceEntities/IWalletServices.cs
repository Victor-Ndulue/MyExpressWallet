namespace Services.Interfaces.IServiceEntities
{
    public interface IWalletServices
    {
        Task CreateWalletAccount(string Email);
    }
}
