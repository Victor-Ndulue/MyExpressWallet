using Application.UnitOfWork.Interfaces;
using Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Services.Helpers.TransactionHelpers;
using Services.Interfaces.IServiceEntities;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Services.Implementations.ServiceEntities
{
    public class VerifyPaymentService : IVerifyPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public VerifyPaymentService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<string> VerifyPayment(string reference, string userEmail, string walletId)
        {
            var transaction = await GetTransactionByReferenceAsync(reference);

            if (transaction == null)
            {
                // Handle transaction not found
                return "Transaction not found.";
            }

            var verifyResponse = await VerifyTransactionAsync(reference);

            if (verifyResponse == null)
            {
                // Handle verification request failure
                return "Verification request failed.";
            }

            if (verifyResponse.success)
            {
                var wallet = await GetUserWalletAsync(userEmail, walletId);

                if (wallet == null)
                {
                    // Handle wallet not found
                    return "Wallet not found.";
                }

                var updatedBalance = wallet.Balance + verifyResponse.amount;
                UpdateTransactionAndWallet(transaction, wallet, verifyResponse.amount, updatedBalance);

                return "Wallet successfully funded";
            }

            return verifyResponse.message;
        }

        private async Task<Transaction> GetTransactionByReferenceAsync(string reference)
        {
            return await _unitOfWork.TransactionQuery.GetTransactionsByReference(reference, false);
        }

        private async Task<VerifyResponse> VerifyTransactionAsync(string reference)
        {
            var url = $"https://api.paystack.co/transaction/verify/{reference}";
            var authorization = _configuration["Payment:PaystackSK"];

            using (var apiClient = new HttpClient())
            {
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization) ;
                apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var response = await apiClient.GetAsync(url))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var stringResponse = await response.Content.ReadAsStringAsync();
                        return JsonSerializer.Deserialize<VerifyResponse>(stringResponse);
                    }

                    return null; // Handle verification request error
                }
            }
        }

        private async Task<UserWallet> GetUserWalletAsync(string userEmail, string walletId)
        {
            return await _unitOfWork.UserWalletQuery.GetUserWalletsByUserEmail(userEmail, false)
                .SingleOrDefaultAsync(x => x.Id == walletId);
        }

        private void UpdateTransactionAndWallet(Transaction transaction, UserWallet wallet, decimal amount, decimal updatedBalance)
        {
            transaction.Status = "successful";
            transaction.Amount = amount;
            transaction.RemainingBalance = updatedBalance;

            wallet.Balance = updatedBalance;

            _unitOfWork.TransactionCommand.Update(transaction);
            _unitOfWork.UserWalletCommand.Update(wallet);
            _unitOfWork.SaveChangesAsync();
        }

    }
}
