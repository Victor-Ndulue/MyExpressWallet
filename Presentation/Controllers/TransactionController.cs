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
            var userMail = User.GetUsername();
            var response = await _transactionServices.VerifyPaymentService.VerifyPayment(reference,userMail,walletId);
            return Ok(response);
        }

        [HttpPost]
        [Route("fundwallet")]
        public async Task<IActionResult> FundWallet(int amount, string walletId)
        {
            var email = User.GetUsername();
            var response = await _transactionServices.WalletFundingService.FundWalletAccount(amount, email, walletId);
            return Ok(response);
        }
    }
}
