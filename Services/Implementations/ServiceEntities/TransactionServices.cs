using Application.UnitOfWork.Interfaces;
using Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Services.Helpers;
using Services.Helpers.TransactionHelpers;
using Services.Helpers.TransactionInitializeHelpers;
using Services.Interfaces.IServiceEntities;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Services.Implementations.ServiceEntities
{
    public class TransactionServices : ITransactionServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public TransactionServices(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<string> FundWalletAccount(int requestAmount, string userEmail, string walletId)
        {
            var wallet = await _unitOfWork.UserWalletQuery.GetUserWalletsByUserEmail(userEmail, false).SingleOrDefaultAsync(x=>x.Id==walletId);
            ApiHelper.InitializeClient();
            var apiClient = ApiHelper.ApiClient;
            string apiUrl = "https://api.paystack.co/transaction/initialize";
            var authorization = _configuration["Payment:PaystackSK"];
            string Callback_url = "https://localhost:7134/api/Transaction/verify";
            var Amount = requestAmount * 100;
            var Reference = GenerateReference();
            var request = new {
                    email = "ndulue.victor.c@gmail.com",
                    amount = Amount,
                    currency = "NGN",
                    reference = Reference,
                    callback_url = Callback_url
                };
            var jsonRequest = JsonSerializer.Serialize(request);
            var data = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
            apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using (var apiResponse = await apiClient.PostAsync(apiUrl, data))
            {
                if (apiResponse.IsSuccessStatusCode)
                {
                    var stringApiResponse = await apiResponse.Content.ReadAsStringAsync();
                    var initializeResponse = JsonSerializer.Deserialize<InitializeResponse>(stringApiResponse);
                    if (initializeResponse.status != false)
                    {
                        Transaction transaction = new()
                        {
                            TransactionType = "FundWallet",
                            TransactionRef = Reference,
                            SenderUserWallet = wallet,
                            Status = "pending",
                            RemainingBalance = wallet.Balance + requestAmount,
                            Amount = requestAmount,
                            RecipientUserWallet = wallet,
                        };

                        await _unitOfWork.TransactionCommand.CreateAsync(transaction);
                        wallet.SentTransactions.Add(transaction);
                        wallet.Balance += requestAmount;
                        _unitOfWork.UserWalletCommand.Update(wallet);
                        await _unitOfWork.SaveChangesAsync();
                        var authorizationUrl = initializeResponse.authorization_url;
                        return authorizationUrl;
                    }
                    return initializeResponse.message;
                }
                else return apiResponse.ReasonPhrase;
            }
        }

        public async Task<string> VerifyPayment(string reference, string userEmail, string walletId)
        {
            var transaction =await _unitOfWork.TransactionQuery.GetTransactionsByReference(reference, false);
            string url = $"https://api.paystack.co/transaction/verify/{reference}";
            var authorization = _configuration["Payment: PaystackSK"];
            var apiClient = ApiHelper.ApiClient;
            apiClient.DefaultRequestHeaders.Add("authorization", authorization);
            using (HttpResponseMessage response =await apiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    VerifyResponse verify = JsonSerializer.Deserialize<VerifyResponse>(stringResponse);
                    if (verify.success==true)
                    {
                        var wallet = await _unitOfWork.UserWalletQuery.GetUserWalletsByUserEmail(userEmail, false).SingleOrDefaultAsync(x => x.Id == walletId);
                        wallet.Balance += verify.amount;
                        transaction.Status = "successful";
                        transaction.Amount = verify.amount;
                        transaction.RemainingBalance = verify.amount+wallet.Balance;
                        wallet.Balance += verify.amount;
                        _unitOfWork.TransactionCommand.Update(transaction);
                        _unitOfWork.UserWalletCommand.Update(wallet);
                        await _unitOfWork.SaveChangesAsync();
                        return "Wallet successfully funded";
                    }
                    return verify.message;
                }
                return response.ReasonPhrase;
            }
        }

        public string GenerateReference()
        {
            string Ref = Guid.NewGuid().ToString().Substring(0, 6); 
            return Ref;
        }
    }
}
