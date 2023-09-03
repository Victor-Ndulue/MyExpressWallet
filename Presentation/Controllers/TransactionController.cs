using Microsoft.AspNetCore.Mvc;
using Services.Interfaces.IServiceEntities;

namespace Presentation.Controllers
{
    public class TransactionController : BaseController
    {
        private readonly ITransactionServices _transactionServices;

        public TransactionController(ITransactionServices transactionServices)
        {
            _transactionServices = transactionServices;
        }

        [HttpPost]
        [Route("verify")]
        public async Task<IActionResult> VerifyPayment(string reference, string walletId) 
        {
            var userMail = User.GetUserEmail();
            var response = await _transactionServices.VerifyPaymentService.VerifyPayment(reference,userMail,walletId);
            return Ok(response);
        }

        [HttpPost]
        [Route("fundwallet")]
        public async Task<IActionResult> FundWallet(int amount, string walletId)
        {
            var email = User.GetUserEmail();
            var response = await _transactionServices.WalletFundingService.FundWalletAccount(amount, email, walletId);
            return Ok(response);
        }

        [HttpGet("get-by-reference/transactionref")]
        public async Task<IActionResult> GetTransactionByRefernce([FromQuery] string reference)
        {
            var userMail = User.GetUserEmail();
            var result = await _transactionServices.GetTransactionByReferenceForUser(reference, userMail);
            return Ok(result);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllTransactions()
        {
            var userMail = User.GetUserEmail();
            var result = await _transactionServices.GetAllTransactions(userMail);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteTransaction (string transactionRef)
        {
            var userMail = User.GetUserEmail();
            var result = await _transactionServices.DeleteTransaction(transactionRef, userMail);
            return Ok(result);
        }
    }
}
