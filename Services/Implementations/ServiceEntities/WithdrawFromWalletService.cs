using Application.UnitOfWork.Interfaces;
using AutoMapper;
using Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers;
using Services.Helpers.MailServices;
using Services.Helpers.TransactionHelpers;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;
using Services.LoggerService.Exceptions.EntityExceptions.EntityNotFoundException;
using Services.LoggerService.Interface;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Services.Implementations.ServiceEntities
{
    public class WithdrawFromWalletService : IWithdrawFromWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceManager _services;
        private readonly IEmailService _emailService;


        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        public WithdrawFromWalletService(IUnitOfWork unitOfWork, IServiceManager serviceManager, IConfiguration configuration, IMapper mapper, ILoggerManager logger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _services = serviceManager;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<StandardResponse<TransactionResponseDto>> WithdrawFromWalletWithBankNameAndAccNumber(WithdrawalRequestDto withdrawalRequest, string userEmail, string bankCode)
        {
            var userResponse = await _services.UserServices.GetUserWithEmail(userEmail);
            var verifyDetails = new UserLoginRequestDto(userResponse.Data.UserName, withdrawalRequest.password);
            var authorizeUser = await _services.AuthenticationServices.UserLogin(verifyDetails);
            if (authorizeUser.Succeeded)
            {
                var confirmWallet = await _services.WalletServices.GetUserWalletById(withdrawalRequest.walletId);

                var accountDetails = await ResolveBankAccount(withdrawalRequest.accountNumber, withdrawalRequest.bankName, bankCode);


                //Convert names to array to compare
                string[] accountNameWords = accountDetails.data.account_name.Split(' ');
                string[] requestNameWords = withdrawalRequest.accountName.Split(' ');

                //Converting alphabets to Upper case and back to array.
                accountNameWords = accountNameWords.Select(word => word.ToUpper()).ToArray();
                requestNameWords = requestNameWords.Select(word => word.ToUpper()).ToArray();

                bool compareNames = requestNameWords.All(word => accountNameWords.Contains(word));

                if (compareNames)
                {
                    var recipient = await CreateTransferRecipient(accountDetails);
                    var recipientAuth = recipient.data.recipient_code;
                    var withdrawResponse = await WithdrawFromWallet(withdrawalRequest.amount, withdrawalRequest.walletId, userEmail, recipientAuth);

                    var subject = "Withdrawal From MyExpressWallet";
                    var message = $"The sum of {withdrawalRequest.amount} was withdrawn from your account. Respond to this mail if you did not initiate request.";
                    await _emailService.SendEmailAsync(userEmail, subject, message);
                    return withdrawResponse;
                }
                throw new ArgumentException("Invalid bank account details. Check and try again");
            }
            throw new UnauthorizedAccessException("Sender credential verification failed. Check Password and try again");
        }

        public async Task<StandardResponse<TransactionResponseDto>> WithdrawFromWallet(int amount, string walletId, string userEmail, string recipientAuth)
        {

            var wallet = await GetUserWalletAsync(walletId);
            if (wallet == null) throw new UserWalletNotFoundException("walletId", walletId);

            var requestAmountAndCharge = amount + (decimal)(amount * 0.04);
            if (wallet.Balance <= requestAmountAndCharge) throw new ArgumentException("Insufficient balance");

            var initializeResponse = await InitializeTransfer(amount, recipientAuth);
            if (initializeResponse.Succeeded)
            {
                //Note this mapping needs revisiting. May not work.
                var paymentRecord = _mapper.Map<PaymentRecord>(initializeResponse);

                var transactionType = "Withdraw";
                var transactionRef = initializeResponse.Data.data.reference;
                var status = "Pending";
                var remainingBalance = wallet.Balance - requestAmountAndCharge;

                await _services.WalletServices.UpdateWallet(walletId, remainingBalance, null);

                var transactionRequestDto = new TransactionCreationRequestDto
                {
                    TransactionType = transactionType,
                    TransactionRef = transactionRef,
                    Status = status,
                    RemainingBalance = wallet.Balance,
                    Amount = amount,
                };
                var transactionResponse = await _services.TransactionServices.CreateTransaction(transactionRequestDto, walletId, null);
                await _unitOfWork.PaymentRecordCommandRepo.CreateAsync(paymentRecord);
                await _unitOfWork.SaveChangesAsync();
                return StandardResponse<TransactionResponseDto>.Success("success", transactionResponse.Data);
            }
            return StandardResponse<TransactionResponseDto>.Failed(initializeResponse.Message);
        }

        private async Task<StandardResponse<InitializeTransferResponse>> InitializeTransfer(int amount, string userAuth)
        {
            var apiUrl = "https://api.paystack.co/transfer";
            var auth = _configuration["Payment:PaystackSK"];
            var stringRef = Guid.NewGuid().ToString().Substring(0, 17);
            var mainReference = Regex.Replace(stringRef, "[^a-zA-Z0-9]", "T");
            var koboAmount = amount * 100;

            var requestData = new
            {
                source = "balance",
                currency = "NGN",
                Reference = mainReference,
                amount = koboAmount,
                reason = "Credit from MyExpressWallet",
                recipient = $"{userAuth}"
            };
            var serializeRequestData = JsonSerializer.Serialize(requestData);
            var data = new StringContent(serializeRequestData, Encoding.UTF8, "application/json");

            //Returning Bad request
            using (var apiClient = new HttpClient())
            {
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth);
                apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var apiResponse =  await apiClient.PostAsync(apiUrl, data);

                if (apiResponse.IsSuccessStatusCode)
                {
                    var initializeTransferResponse = await apiResponse.Content.ReadAsAsync<InitializeTransferResponse>();
                    return StandardResponse<InitializeTransferResponse>.Success("success", initializeTransferResponse);
                }

                else
                {
                    var statusCode = (int)apiResponse.StatusCode;
                    var reasonPhrase = apiResponse.ReasonPhrase;
                    var innerException = new Exception($"HTTP request failed with status code {statusCode}: {reasonPhrase}");

                    return StandardResponse<InitializeTransferResponse>.Failed("Failed to confirm transaction at the moment. Please try again");
                }
            }
        }

        public async Task<ResolveAccountDetailsPaystackResponse> ResolveBankAccount(string accountNumber, string bankName, string bankCode)
        {
            bankName = bankName.ToUpper();
            //var bankCode = await GetBankCode(bankName);

            var apiUrl = $"https://api.paystack.co/bank/resolve?account_number={accountNumber}&bank_code={bankCode}";
            var authorization = _configuration["Payment:PaystackSK"];

            using (var apiClient = new HttpClient())
            {
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
                using (var response = await apiClient.GetAsync(apiUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var accountDetails = await response.Content.ReadAsAsync<ResolveAccountDetailsPaystackResponse>();
                        accountDetails.data.bank_code = bankCode;
                        return accountDetails;
                    }
                    else
                    {
                        var statusCode = (int)response.StatusCode;
                        var reasonPhrase = response.ReasonPhrase;
                        var innerException = new Exception($"HTTP request failed with status code {statusCode}: {reasonPhrase}");

                        throw new HttpProtocolException(statusCode, reasonPhrase, innerException);
                    }
                }
            }

        }

        private async Task<TransferRecipientResponse> CreateTransferRecipient(ResolveAccountDetailsPaystackResponse accountDetails)
        {
            var apiUrl = "https://api.paystack.co/transferrecipient";
            var auth = _configuration["Payment:PaystackSK"];
            var requestData = new
            {
                type = "nuban",
                name = accountDetails.data.account_name,
                account_number = accountDetails.data.account_number,
                bank_code = accountDetails.data.bank_code,
                currency = "NGN"
            };

            var serializeRequestData = JsonSerializer.Serialize(requestData);
            var data = new StringContent(serializeRequestData, Encoding.UTF8, "application/json");

            using (var apiResponse = await ApiHelper.PostRequestAsync(apiUrl, data, auth))
            {
                if (apiResponse.IsSuccessStatusCode)
                {
                    var transferRecipientResponse = await apiResponse.Content.ReadAsAsync<TransferRecipientResponse>();
                    return transferRecipientResponse;
                }
                else
                {
                    var statusCode = (int)apiResponse.StatusCode;
                    var reasonPhrase = apiResponse.ReasonPhrase;
                    var innerException = new Exception($"HTTP request failed with status code {statusCode}: {reasonPhrase}");

                    throw new HttpProtocolException(statusCode, reasonPhrase, innerException);
                }
            }
        }

        private async Task<string> GetBankCode(string bankName)
        {
            var bankList = await GetBankList();
            var bankData = bankList.data;
            string bankCode;

            string[] requestBankNameWords = bankName.Split(' ');

            bool compareBankNames = true;

            foreach (var bank in bankData)
            {
                string[] bankNameWords = bank.name.ToUpper().Split(' ');

                for (int i = 0; i < requestBankNameWords.Length; i++)
                {
                    if (requestBankNameWords[i] != bankNameWords[i])
                    { compareBankNames = false; break; }
                    if (compareBankNames) { bankCode = bank.code; return bankCode; }
                }
            }
            return null;
        }

        public async Task<BankListResponse> GetBankList()
        {

            var authorization = _configuration["Payment:PaystackSK"];

            var url = $"https://api.paystack.co/bank?currency=NGN";

            using (var apiClient = new HttpClient())
            {
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
                apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (var response = await apiClient.GetAsync(url))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var bankListResponse = await response.Content.ReadAsAsync<BankListResponse>();
                        return bankListResponse;
                    }
                    else
                    {
                        var statusCode = (int)response.StatusCode;
                        var reasonPhrase = response.ReasonPhrase;
                        var innerException = new Exception($"HTTP request failed with status code {statusCode}: {reasonPhrase}");

                        throw new HttpProtocolException(statusCode, reasonPhrase, innerException);
                    }
                }
            }
        }

        private async Task<WalletResponseDto> GetUserWalletAsync(string walletId)
        {
            var response = await _services.WalletServices.GetUserWalletById(walletId);
            return response.Data;
        }

    }
}
