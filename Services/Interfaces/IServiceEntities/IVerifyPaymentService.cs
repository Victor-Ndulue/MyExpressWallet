namespace Services.Interfaces.IServiceEntities
{
    public interface IVerifyPaymentService
    {
        Task<string> VerifyPayment(string reference, string userEmail, string walletId);
    }
}
