using Microsoft.AspNetCore.Mvc;
using Services.Interfaces.IServiceEntities;

namespace Presentation.Controllers
{
    public class WalletController : BaseController
    {
        private readonly IWalletServices _walletServices;
        private readonly ITransactionServices _transactionServices;

        public WalletController(IWalletServices walletServices, ITransactionServices transaction)
        {
            _walletServices = walletServices;
            _transactionServices = transaction;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateWalletAccount()
        {
            var userMail = User.GetUsername();
             await _walletServices.CreateWalletAccount(userMail);
            return Ok();
        }

        [HttpPost]
        [Route("fundwallet")]
        public async Task<IActionResult> FundWallet(int amount, string walletId)
        {
            var email = User.GetUsername();
            var response = await _transactionServices.FundWalletAccount(amount, email, walletId);
            return Ok(response);
        }
    }
}
