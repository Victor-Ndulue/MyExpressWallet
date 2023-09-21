using Microsoft.AspNetCore.Mvc;
using Services.DTO_s.Request;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;
using Shared.PaginationDefiners;

namespace Presentation.Controllers
{
    public class WalletController : BaseController
    {
        private readonly IWalletServices _walletServices;
        private readonly IVerifyWalletFundingService _verifyWalletFundingService;
        private readonly IWithdrawFromWalletService _withdrawFromWalletService;
        private readonly IWalletFundingService _walletFundingService;

        public WalletController(IServiceManager services)
        {
            _walletServices = services.WalletServices;
            _verifyWalletFundingService = services.VerifyWalletFundingService;
            _withdrawFromWalletService = services.WithdrawFromWalletService;
            _walletFundingService = services.WalletFundingService;
        }
        /// <summary>
        /// Creates wallet account for user
        /// </summary>
        /// <returns> respose 200 with data and status code</returns>
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateWalletAccount()
        {
            var userId = User.GetUserId();
            var result = await _walletServices.CreateWalletAccount(userId);
            return new OkObjectResult(result);
        }

        /// <summary>
        /// Funds the wallet
        /// </summary>
        /// <param name="requestAmount"></param>
        /// <param name="walletId"></param>
        /// <returns>a confirmation message data.</returns>
        [HttpPost("fundwallet")]
        public async Task<IActionResult> FundWalletAccount([FromForm] int requestAmount, string walletId)
        {
            var userEmail = User.GetUserEmail();
            var result = await _walletFundingService.FundWalletAccount(requestAmount, userEmail, walletId);
            return Ok(result);
        }

        /// <summary>
        /// Verifies paymentand updates user wallet
        /// </summary>
        /// <param name="transactionReference"></param>
        /// <param name="walletId"></param>
        /// <returns></returns>
        [HttpPost("verify-wallet-funding")]
        public async Task<IActionResult> VerifyPayment([FromForm] string transactionReference, string walletId)
        {
            var userEmail = User.GetUserEmail();
            var result = await _verifyWalletFundingService.VerifyPayment(transactionReference, userEmail, walletId);
            return Ok(result);
        }

        /// <summary>
        /// Debits wallet to fund bankaccount with entered details
        /// </summary>
        /// <param name="withdrawalRequest">Details of withdraw and validation</param>
        /// <param name="bankCode">code of bank</param>
        /// <returns></returns>
        [HttpPost("withdraw-from-wallet")]
        public async Task<IActionResult> WithdrawFromWalletToBank(WithdrawalRequestDto withdrawalRequest, string bankCode)
        {
            var userEmail = User.GetUserEmail();
            var result = await _withdrawFromWalletService.WithdrawFromWalletWithBankNameAndAccNumber(withdrawalRequest, userEmail, bankCode);
            return Ok(result);
        }


        /// <summary>
        /// Resolves account with enterd details
        /// </summary>
        /// <param name="accountNumber">Account number of recipient</param>
        /// <param name="bankName">Recipient bank name</param>
        /// <param name="bankCode">code of Recipient bank</param>
        /// <returns></returns>
        [HttpGet("resolve-account-details")]
        public async Task<IActionResult> ResolveAccountDetails([FromForm] string accountNumber, string bankName, string bankCode)
        {
            var result = await _withdrawFromWalletService.ResolveBankAccount(accountNumber, bankName, bankCode);
            return Ok(result);
        }

        /// <summary>
        /// Gets list of supported banks
        /// </summary>
        /// <returns>List of banks</returns>
        [HttpGet("get-bank-list")]
        public async Task<IActionResult> GetListOfBanksFunded()
        {
            var result = await _withdrawFromWalletService.GetBankList();
            return Ok(result);
        }

        /// <summary>
        /// send money from wallet to another.
        /// </summary>
        /// <param name="requestDetails">details required for validation and transfer</param>
        /// <returns></returns>
        [HttpPost("transfer/wallet-to-wallet")]
        public async Task<IActionResult> SendMoneyToAnotherWallet([FromForm]SendMoneyRequestDto requestDetails) 
        {
            var userEmail = User.GetUserEmail();
            var result = await _walletServices.SendMoneyAsync(requestDetails, userEmail);
            return Ok(result);
        }

        
        /// <summary>
        /// Gets user wallet by wallet id
        /// </summary>
        /// <param name="walletId">Wallet id to search for</param>
        /// <returns>wallet details</returns>
        [HttpGet("by-id")]
        public async Task<IActionResult> GetWalletById([FromQuery]string walletId)
        {
            var result = await _walletServices.GetWalletById(walletId);
            return Ok(result);
        }


        /// <summary>
        /// Get all user wallets
        /// </summary>
        /// <returns>All user wallet details</returns>
        [HttpGet]
        [Route("all-wallets")]
        public async Task<IActionResult> GetAllWallets([FromQuery] PaginationParams pagination)
        {
            var result = await _walletServices.GetAllWallets(pagination);
            return Ok(result);
        }

        /// <summary>
        /// Gets wallet for a particular user
        /// </summary>
        /// <returns>User wallet details</returns>
        [HttpGet]
        [Route("user/all-wallets")]
        public async Task<IActionResult> GetAllUserWallets([FromQuery] PaginationParams pagination)
        {
            var userMail = User.GetUserEmail();
            var result = await _walletServices.GetAllUserWallets(userMail, pagination);
            return Ok(result);
        }

        /// <summary>
        /// deletes wallet account
        /// </summary>
        /// <param name="walletId"></param>
        /// <returns>a string response</returns>
        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteUserWalletAccount([FromForm] string walletId)
        {
            var result = await _walletServices.DeleteUserWallet(walletId);
            return Ok(result);
        }
    }
}
