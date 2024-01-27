using Microsoft.Extensions.Configuration;
using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers;
using Services.Helpers.MailServices;
using Services.Helpers.TransactionInitializeHelpers;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;
using Services.LoggerService.Exceptions.EntityExceptions.EntityNotFoundException;
using Services.LoggerService.Interface;
using System.Text;
using System.Text.Json;

namespace Services.Implementations.ServiceEntities
{
    public class WalletFundingService : IWalletFundingService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerManager _logger;

        private readonly IServiceManager _serviceManager;
        private readonly IEmailService _emailService;

        public WalletFundingService(IConfiguration configuration, ILoggerManager logger, IServiceManager serviceManager, IEmailService emailService)
        {
            _configuration = configuration;
            _logger = logger;
            _serviceManager = serviceManager;
            _emailService = emailService;
        }

        public async Task<StandardResponse<InitializeWalletFundingResponseDto>> FundWalletAccount(int requestAmount, string userEmail, string walletId)
        {
            _logger.LogInfo("Started protocol to fund wallet");

            _logger.LogInfo($"Attempting to find user wallet with id {walletId} to fund");
            var wallet = await _serviceManager.WalletServices.GetUserWalletById(walletId);

            if (wallet.Data == null)
            {
                _logger.LogError($"user wallet with id {walletId} not found. Throwing exception to user");
                throw new UserWalletNotFoundException("wallet id", walletId);
            }

            var initializeResponse = await InitializePaymentAsync(userEmail,requestAmount);
            if (initializeResponse.Succeeded)
            {

                var transactionRef = initializeResponse.Data.data.reference;
                var authorizationUrl = initializeResponse.Data.data.authorization_url;
                var walletBalance = wallet.Data.Balance;

                _logger.LogInfo("creating transaction data");
                var transactionType = "FundWallet";
                var status = "Pending";

                var transactionRequestDto = new TransactionCreationRequestDto
                {
                    TransactionType = transactionType,
                    TransactionRef = transactionRef,
                    Status = status,
                    RemainingBalance = walletBalance,
                    Amount = requestAmount
                };
                await _serviceManager.TransactionServices.CreateTransaction(transactionRequestDto, null, walletId);

                var initializeWalletFundingResponse = new InitializeWalletFundingResponseDto();
                initializeWalletFundingResponse.TransactionReference = transactionRef;
                initializeWalletFundingResponse.WalletId = walletId;
                initializeWalletFundingResponse.AuthorizationUrl = authorizationUrl;

                var subject = "MyExpreeWallet Successfully Funded";
                initializeWalletFundingResponse.AuthorizationUrl = authorizationUrl;
                var message = $"<html><body> <br> Wallet funding initialized with reference {transactionRef}. Please complete the process to verify your deposit.\n Click to complete process or copy link {authorizationUrl} to browser to complete. <a href = {authorizationUrl}>  Click </a><br /></body></html>";

                await _emailService.SendEmailAsync(userEmail, subject, message);

                _logger.LogInfo("returning authorization url to user for payment");
                return StandardResponse<InitializeWalletFundingResponseDto>.Success("successful", initializeWalletFundingResponse);
            }
            return StandardResponse<InitializeWalletFundingResponseDto>.Failed(initializeResponse.Message, initializeResponse.StatusCode);
        }        

        private async Task<StandardResponse<InitializeTransactionResponse>> InitializePaymentAsync(string email, int requestAmount)
        {
            _logger.LogInfo("started protocol to initialize payment for user to paystack");

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
            var data = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            _logger.LogWarn($"sending request to paystack with request data {data}");
            using (var apiResponse = await ApiHelper.PostRequestAsync(apiUrl, data, authorization))
            {
                if (apiResponse.IsSuccessStatusCode)
                {
                    var initializeTransactionResponse = await apiResponse.Content.ReadAsAsync<InitializeTransactionResponse>();
                    _logger.LogInfo($"succeeded connecting to paystack, returning response {await apiResponse.Content.ReadAsAsync<string>()}");
                    return StandardResponse<InitializeTransactionResponse>.Success("success", initializeTransactionResponse);
                }
                else
                {
                    var statusCode = (int)apiResponse.StatusCode;
                    var reasonPhrase = apiResponse.ReasonPhrase;
                    var innerException = new Exception($"HTTP request failed with status code {statusCode}: {reasonPhrase}");

                    _logger.LogError($"failed to initialize payment to paystack. Throwing exception: {innerException}");
                    return StandardResponse<InitializeTransactionResponse>.Failed("service not available at the moment. Try again later.", 500);
                }
            }
        }     
                
        private string GenerateReference()
        {
            string Ref = Guid.NewGuid().ToString().Substring(0,9); 
            return Ref;
        }
    }
}
