using Application.UnitOfWork.Interfaces;
using Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Services.Helpers;
using Services.Helpers.TransactionInitializeHelpers;
using Services.Interfaces.IServiceEntities;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Services.Implementations.ServiceEntities
{
    public class WalletFundingService : IWalletFundingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public WalletFundingService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<StandardResponse<string>> FundWalletAccount(int requestAmount, string userEmail, string walletId)
        {
            var wallet = await GetUserWalletAsync(userEmail, walletId);

            if (wallet == null)
            {
                return StandardResponse<string>.Failed("Wallet not found.");
            }

            var initializeResponse = await InitializePaymentAsync(userEmail,requestAmount);

            if (initializeResponse == null || !initializeResponse.status)
            {
                return StandardResponse<string>.Failed( "Payment initialization failed." + initializeResponse.message);
            }

            var transactionRef = GenerateReference();
            var authorizationUrl = initializeResponse.authorization_url;

            var transaction = CreateTransaction(wallet, requestAmount, transactionRef);

            await UpdateWalletAndTransactionAsync(wallet, transaction, requestAmount);

            return StandardResponse<string>.Success("successful", authorizationUrl);
        }

        private async Task<UserWallet> GetUserWalletAsync(string userEmail, string walletId)
        {
            return await _unitOfWork.UserWalletQuery
                .GetUserWalletsByUserEmail(userEmail, false)
                .SingleOrDefaultAsync(x => x.Id == walletId);
        }

        private async Task<InitializeResponse> InitializePaymentAsync(string email,int requestAmount)
        {
            var apiUrl = "https://api.paystack.co/transaction/initialize";
            var authorization = _configuration["Payment:PaystackSK"];
            var callbackUrl = "https://localhost:7134/api/Transaction/verify";

            var request = new
            {
                email = email,
                amount = requestAmount * 100,
                currency = "NGN",
                reference = GenerateReference(),
                callback_url = callbackUrl
            };

            var jsonRequest = JsonSerializer.Serialize(request);
            var data = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            using (var apiResponse = await SendPaymentRequestAsync(apiUrl, data, authorization))
            {
                if (apiResponse.IsSuccessStatusCode)
                {
                    var stringApiResponse = await apiResponse.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<InitializeResponse>(stringApiResponse);
                }

                return null; // Handle error response
            }
        }

        private async Task<HttpResponseMessage> SendPaymentRequestAsync(string apiUrl, HttpContent data, string authorization)
        {
            using (var apiClient = new HttpClient())
            {
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
                apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return await apiClient.PostAsync(apiUrl, data);
            }
        }

        private Transaction CreateTransaction(UserWallet wallet, int requestAmount, string transactionRef)
        {
            return new Transaction
            {
                TransactionType = "FundWallet",
                TransactionRef = transactionRef,
                SenderUserWallet = wallet,
                Status = "pending",
                RemainingBalance = wallet.Balance + requestAmount,
                Amount = requestAmount,
                RecipientUserWallet = wallet
            };
        }

        private async Task UpdateWalletAndTransactionAsync(UserWallet wallet, Transaction transaction, int requestAmount)
        {
            await _unitOfWork.TransactionCommand.CreateAsync(transaction);
            wallet.SentTransactions.Add(transaction);
            wallet.Balance += requestAmount;
            _unitOfWork.UserWalletCommand.Update(wallet);
            await _unitOfWork.SaveChangesAsync();
        }
        
        private string GenerateReference()
        {
            string Ref = Guid.NewGuid().ToString().Substring(0, 6); 
            return Ref;
        }
    }
}
