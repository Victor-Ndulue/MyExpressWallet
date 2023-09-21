using Application.UnitOfWork.Interfaces;
using Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Services.DTO_s.Response;
using Services.Helpers;
using Services.Helpers.MailServices;
using Services.Helpers.TransactionHelpers;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;
using Services.LoggerService.Exceptions.EntityExceptions.EntityNotFoundException;
using Services.LoggerService.Interface;
using System.Net.Http.Headers;

namespace Services.Implementations.ServiceEntities
{
    public class VerifyWalletFundingService : IVerifyWalletFundingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILoggerManager _logger;

        private readonly IServiceManager _serviceManager;
        private readonly IEmailService _emailService;

        public VerifyWalletFundingService(IUnitOfWork unitOfWork, IConfiguration configuration, ILoggerManager logger, IServiceManager serviceManager, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
            _serviceManager = serviceManager;
            _emailService = emailService;
        }

        public async Task<StandardResponse<string>> VerifyPayment(string transactionReference, string userEmail, string walletId)
        {            
            _logger.LogInfo($"started protocol to verify payment for wallet with id {walletId}");

            var transactionResponse = await _serviceManager.TransactionServices.GetTransactionByReferenceForUser(transactionReference, userEmail, walletId);
            var transactionRespnseData = transactionResponse.Data;

            if (transactionRespnseData == null)
            {
                _logger.LogError($"no wallet transaction with reference {transactionReference} found");
                throw new TransactionNotFoundException("reference", transactionReference);
            }

            if (transactionRespnseData.Status == "successful") { return StandardResponse<string>.Failed( "transaction had already been verified"); }

            var verifyResponse = await VerifyTransactionAsync(transactionReference);
            if (verifyResponse.Succeeded)
            {

                if (verifyResponse.Data.data.gateway_response == "Successful")
                {
                    var wallet = await GetUserWalletAsync(walletId);

                    if (wallet == null)
                    {
                        throw new UserWalletNotFoundException("wallet id", walletId);
                    }

                    var amount = verifyResponse.Data.data.amount / 100;
                    var updatedBalance = wallet.Balance + amount;
                    var payStackAuth = verifyResponse.Data.data.authorization.authorization_code;

                    await _serviceManager.TransactionServices.UpdateTransaction(transactionReference, amount, updatedBalance);
                    await _serviceManager.WalletServices.UpdateWallet(walletId, updatedBalance, payStackAuth);


                    var subject = "Deposit Verification";
                    var message = $"Dear {transactionRespnseData.RecipientUserName}. \n Deposit of  {amount} to your wallet had been successfully verified. " +
                        $"You can use this reference {transactionReference} to see transaction details";
                    await _emailService.SendEmailAsync(userEmail, subject, message);

                    return StandardResponse<string>.Success("success", "Wallet successfully funded");
                }

                return StandardResponse<string>.Failed($"wallet payment with transaction ref {transactionReference} unverified. Reason: {verifyResponse.Data.data.gateway_response}");
            }
            return StandardResponse<string>.Failed(verifyResponse.Message);
        }        

       
        private async Task<StandardResponse<VerifyTransactionResponse>> VerifyTransactionAsync(string reference)
        {
            _logger.LogInfo($"started protocol to verify payment with ref {reference} with paystack");

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
                        _logger.LogInfo($"successfully verified payment with paystack. Returning data");
                        var verifyTransactionResponse = await response.Content.ReadAsAsync<VerifyTransactionResponse>();
                        return StandardResponse<VerifyTransactionResponse>.Success("success", verifyTransactionResponse);
                    }

                    else
                    {
                        var statusCode = (int)response.StatusCode;
                        var reasonPhrase = response.ReasonPhrase;
                        var innerException = new Exception($"HTTP request failed with status code {statusCode}: {reasonPhrase}");

                        _logger.LogError($"failed to verify payment with paystack. Throwing exception: {innerException}");
                        return StandardResponse<VerifyTransactionResponse>.Failed("failed to verify transaction at the moment. Try again later");
                    }
                }
            }
        }

        private async Task<WalletResponseDto> GetUserWalletAsync( string walletId)
        {
            var response = await _serviceManager.WalletServices.GetUserWalletById(walletId);
            return response.Data;
        }       
    }
}
